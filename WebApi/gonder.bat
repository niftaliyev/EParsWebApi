dotnet publish -r linux-x64 --self-contained false -c Release -o deploy

pscp .\deploy\* root@23.88.126.140:/root/Publish/

ssh root@23.88.126.140 'sudo systemctl restart webapi.service'

pause