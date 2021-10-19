using BastardBot.Common.DB;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BastardBot.Common
{
    /// <summary>
    /// The genius brains of this bastard.
    /// </summary>
    public abstract class BastardBrain
    {
        public abstract BastardDBContext GetBastardDBContext();
        public abstract QnAMakerClient GetQnAMakerClient();
        public abstract SystemSettings GetSettings();

        public async Task InitDatabaseAndModel()
        {
            var dbContext = GetBastardDBContext();

            bool newModel = await DbInitialiser.Init(dbContext);
            if (newModel)
            {
                await TrainAndPublishNewModel();
            }
        }


        public async Task<List<InsultResponse>> GetTrainedInsultResponses()
        {
            var _qnAMakerClient = GetQnAMakerClient();
            var _settings = GetSettings();
            var kbData = await _qnAMakerClient.Knowledgebase.DownloadAsync(_settings.QnAKnowledgebaseId, EnvironmentType.Prod);

            var insultsResponses = new List<InsultResponse>();

            foreach (var kbDoc in kbData.QnaDocuments)
            {
                var response = new InsultResponse { Text = kbDoc.Answer };
                var insults = new List<Insult>();
                foreach (var kbInsult in kbDoc.Questions)
                {
                    insults.Add(new Insult { Text = kbInsult, ParentResponse = response });
                }
                response.InsultTriggers = insults;
                insultsResponses.Add(response);
            }

            return insultsResponses;
        }

        public async Task<List<Insult>> GetTrainedInsultsOnly()
        {
            var _qnAMakerClient = GetQnAMakerClient();
            var _settings = GetSettings();
            var kbData = await _qnAMakerClient.Knowledgebase.DownloadAsync(_settings.QnAKnowledgebaseId, EnvironmentType.Prod);

            var insults = new List<Insult>();

            foreach (var kbDoc in kbData.QnaDocuments)
            {
                foreach (var kbInsult in kbDoc.Questions)
                {
                    insults.Add(new Insult { Text = kbInsult });
                }
            }

            return insults;
        }

        public async Task TrainAndPublishNewModel()
        {
            Console.WriteLine("Training new QnA model...");
            var _qnAMakerClient = GetQnAMakerClient();
            var _settings = GetSettings();

            // Load new insults added            
            var dbContext = GetBastardDBContext();
            var newInsultResponses = await dbContext.NewResponses.Include(r => r.InsultTriggers).ToListAsync();

            if (newInsultResponses.Count == 0)
            {
                Console.WriteLine("Nothing to train.");
                return;
            }

            Console.Write("Downloading KB...");
            var kbData = await _qnAMakerClient.Knowledgebase.DownloadAsync(_settings.QnAKnowledgebaseId, EnvironmentType.Prod);
            Console.WriteLine("KB Downloaded. It has {0} QnAs.", kbData.QnaDocuments.Count);

            // Make updates to QnA KB
            UpdateKbOperationDTO updates = GenerateKbUpdatesFromNewInsults(kbData, newInsultResponses);
            bool updatesMadeToKB = await UpdateKB(_qnAMakerClient, _settings.QnAKnowledgebaseId, updates);

            // Publish if there's actually anything do update
            if (updatesMadeToKB)
            {
                // Publish
                _qnAMakerClient.Knowledgebase.PublishAsync(_settings.QnAKnowledgebaseId).Wait();
                Console.WriteLine("Trained QnA model.");
            }
            else
            {
                Console.WriteLine("No new updates to KB.");
            }

            // Clear out pending insults etc
            dbContext.NewResponses.RemoveRange(newInsultResponses);
            var newInsults = await dbContext.NewInsults.ToListAsync();
            dbContext.NewInsults.RemoveRange(newInsults);
            await dbContext.SaveChangesAsync();
        }
        public async Task AddNewInsultQnA(NewInsultQnA newInsultQnA)
        {
            var dbContext = GetBastardDBContext();

            InsultResponse insultResponse = new InsultResponse() { Text = newInsultQnA.InsultResponse };
            foreach (var newInsult in newInsultQnA.Insults)
            {
                dbContext.NewInsults.Add(new Insult() { Text = newInsult, ParentResponse = insultResponse });
            }
            await dbContext.SaveChangesAsync();

        }

        private UpdateKbOperationDTO GenerateKbUpdatesFromNewInsults(QnADocumentsDTO kbData, List<InsultResponse> newInsultResponses)
        {
            List<QnADTO> newQnAs = new List<QnADTO>();

            Dictionary<QnADTO, List<string>> updatesMatrix = new Dictionary<QnADTO, List<string>>();

            // Enumerate insult responses, as we want to check each new response exists or not as an Answer
            foreach (var newInsultResponse in newInsultResponses)
            {
                // Do any of the new insult answers exist in another qna answer-list already?
                QnADTO answerExistingDoc = null;
                foreach (var qnaDoc in kbData.QnaDocuments)
                {
                    if (qnaDoc.Answer.ToLower() == newInsultResponse.Text.ToLower())
                    {
                        // Add insult (question) to existing insult-resoponse (answer)
                        answerExistingDoc = qnaDoc;
                        break;
                    }
                }

                if (answerExistingDoc != null)
                {
                    // Insult response exists. Check each original insult for this response
                    foreach (var responseInsult in newInsultResponse.InsultTriggers)
                    {
                        if (!answerExistingDoc.Questions.Contains(responseInsult.Text))
                        {
                            var answer = answerExistingDoc.Answer;
                            // Add updates to dictionary by answer, so we can compile all updates together at the end
                            if (!updatesMatrix.ContainsKey(answerExistingDoc))
                            {
                                updatesMatrix.Add(answerExistingDoc, new List<string>());
                            }

                            // Add new insult for existing response
                            updatesMatrix[answerExistingDoc].Add(responseInsult.Text);
                        }
                    }
                }
                else
                {
                    // Never seen the insult before, or the response. Add new both.
                    newQnAs.Add(new QnADTO()
                    {
                        Answer = newInsultResponse.Text,
                        Questions = newInsultResponse.InsultTriggers.Select(i => i.Text).ToList()
                    });
                }
            }

            // Build API structure for all updates
            var qnasToUpdate = new List<UpdateQnaDTO>();
            foreach (var update in updatesMatrix.Keys)
            {
                qnasToUpdate.Add(new UpdateQnaDTO
                {
                    Id = update.Id,
                    Source = update.Source,
                    Answer = update.Answer,
                    Questions = new UpdateQnaDTOQuestions
                    {
                        Add = updatesMatrix[update]
                    }
                });
            }

            var changes = new UpdateKbOperationDTO();
            changes.Add = new UpdateKbOperationDTOAdd() { QnaList = newQnAs };
            changes.Update = new UpdateKbOperationDTOUpdate { QnaList = qnasToUpdate };
            return changes;
        }


        private static async Task<bool> UpdateKB(IQnAMakerClient client, string kbId, UpdateKbOperationDTO qnADTOs)
        {
            // Update kb
            bool somethingToDo = qnADTOs.Add.QnaList.Count > 0 || qnADTOs.Update.QnaList.Count > 0;
            if (somethingToDo)
            {
                var updateOp = await client.Knowledgebase.UpdateAsync(kbId, qnADTOs);

                // Loop while operation is success
                await MonitorOperation(client, updateOp);
            }
            else
            {
                Console.WriteLine("Nothing to update.");
            }

            return somethingToDo;

        }

        private static async Task<Operation> MonitorOperation(IQnAMakerClient client, Operation operation)
        {
            // Loop while operation is success
            for (int i = 0;
                i < 20 && (operation.OperationState == OperationStateType.NotStarted || operation.OperationState == OperationStateType.Running);
                i++)
            {
                Console.WriteLine("Waiting for operation: {0} to complete.", operation.OperationId);
                await Task.Delay(5000);
                operation = await client.Operations.GetDetailsAsync(operation.OperationId);
            }

            if (operation.OperationState != OperationStateType.Succeeded)
            {
                throw new Exception($"Operation {operation.OperationId} failed to completed.");
            }
            return operation;
        }

    }

    public class DIBastardBrain : BastardBrain, IDisposable
    {

        //private BastardDBContext _dbContext = null;
        private SystemSettings _settings = null;
        private IServiceScope _serviceScope = null;
        private QnAMakerClient _qnAMakerClient = null;

        public DIBastardBrain(IServiceProvider scopeFactory)
        {
            _serviceScope = scopeFactory.CreateScope();
            _settings = _serviceScope.ServiceProvider.GetService<SystemSettings>() ?? throw new ArgumentNullException(nameof(_settings));

            _qnAMakerClient = new QnAMakerClient(new ApiKeyServiceClientCredentials(_settings.QnASubscriptionKey)) { Endpoint = _settings.QnAMakerHost };
        }


        public void Dispose()
        {
            _serviceScope.Dispose();
        }

        public override BastardDBContext GetBastardDBContext()
        {
            return _serviceScope.ServiceProvider.GetService<BastardDBContext>();
        }

        public override QnAMakerClient GetQnAMakerClient()
        {
            return _qnAMakerClient;
        }

        public override SystemSettings GetSettings()
        {
            return _settings;
        }

    }

    public class FunctionAppBastardBrain : BastardBrain
    {
        BastardDBContext _context;
        private QnAMakerClient _qnAMakerClient = null; 
        SystemSettings _settings;

        public FunctionAppBastardBrain(IConfiguration config)
        {
            _settings = new SystemSettings(config);
            _context = new BastardDBContext(config.GetConnectionString("DefaultConnection"));
            _qnAMakerClient = new QnAMakerClient(new ApiKeyServiceClientCredentials(_settings.QnASubscriptionKey)) { Endpoint = _settings.QnAMakerHost };
        }

        public override BastardDBContext GetBastardDBContext()
        {
            return _context;
        }

        public override QnAMakerClient GetQnAMakerClient()
        {
            return _qnAMakerClient;
        }

        public override SystemSettings GetSettings()
        {
            return _settings;
        }
    }
}
