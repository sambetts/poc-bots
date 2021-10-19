using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;

namespace BastardBot.Common.Abstract
{
    /// <summary>
    /// Base class all database classes inherit from.
    /// </summary>
    public abstract class BaseObject
    {
        [JsonIgnore]
        public int ID { get; set; }

        [BindNever]
        [JsonIgnore]
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IsUnsavedObject
        {
            get { return !(this.ID > 0); }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} ID={ID}";
        }
    }
    public abstract class BaseObjectWithText : BaseObject
    {
        public BaseObjectWithText()
        {
            this.Text = string.Empty;
        }
        public BaseObjectWithText(string name) : this()
        {
            this.Text = name;
        }

        //[Required]
        public string Text { get; set; }


        public override string ToString()
        {
            return $"{this.GetType().Name}: ID={ID}, Name={Text}";
        }
    }
}
