#msbuild Proxy.csproj /t:package /p:Configuration=Release

.\obj\Release\Package\Proxy.deploy.cmd /M:publish.gear.host /U:$ptest /P:P8N8nblb1cFi6aAvayaK1Rnilb2cZsxG9akYsFlruhL5mi8f7juuWeWoxEnl msdeploySite="ptest" destinationAppUrl="http://ptest.gear.host"



#publishMethod="MSDeploy" 
#publishUrl="publish.gear.host" 
#msdeploySite="ptest" 
#userName="$ptest" 
#userPWD="P8N8nblb1cFi6aAvayaK1Rnilb2cZsxG9akYsFlruhL5mi8f7juuWeWoxEnl" 
#destinationAppUrl="http://ptest.gear.host" 
 