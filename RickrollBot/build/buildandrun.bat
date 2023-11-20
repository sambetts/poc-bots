docker build `
--build-arg CallSignalingPort=9441 `
--build-arg CallSignalingPort2=94412 `
--build-arg InstanceInternalPort=8445 `
-f ./build/Dockerfile . -t rickrollbot

docker run -it `
--cpus 2.5 `
--env-file .\src\RecordingBot.Console\.env `
--mount type=bind,source=C:\Users\sambetts\Desktop\BotDirs\Cert,target=C:\certs `
-p 9441:9441 `
-p 9442:9442 `
-p 8445:8445 `
rickrollbot