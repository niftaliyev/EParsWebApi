@echo off
echo "./publish folderin zip ele -> publish.zip"
powershell "Compress-Archive -Force ./publish/* publish.zip"

echo "./publish.zip faylin upload et"
scp ./publish.zip emlakcrawler:/root/publish.zip

echo "./publish.zip faylin sil (artiq upload etmisik)"
del publish.zip

echo "servisi stop et, unzip et, restart et."
ssh emlakcrawler "systemctl stop webapi; unzip -o /root/publish.zip -d /root/publish; rm /root/publish.zip; chmod +x /root/publish/WebApi; systemctl restart webapi"

echo "fso :)"
pause