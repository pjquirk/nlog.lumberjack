# NLog Lumberjack target
NLog target. Sends events (logs, metrics and alerts) using Lumberjack protocol.
##1. Overview
NLog Lumberjack target allows to send logs, metrics and alerts using Lumberjack protocol.
Installation guide: [ubuntu](https://www.digitalocean.com/community/tutorials/how-to-install-elasticsearch-logstash-and-kibana-4-on-ubuntu-14-04), [centos](https://www.digitalocean.com/community/tutorials/how-to-install-elasticsearch-logstash-and-kibana-4-on-centos-7])
##2. Environment configuration
###2.1. Logstash
Configure input Lumberjack plugin for Logstash 
```
input {
  lumberjack {
    port => 5000
    ssl_certificate => "/etc/pki/tls/certs/my.domain.public.key.crt"
    ssl_key => "/etc/pki/tls/private/my.domain.private.key.key"
  }
}
```
###2.2. Install certificate to your system
Install certificate to `Local Machine` store. Select `Automaticaly select the certificate store based on the type of certificate`
###2.3. Copy certificate thumbprint
Double click to `.crt` file, go `Details` tab and copy `Thumbprint` field
##3. NLog target configuration
###3.1. Install package
To install NLog.LumberjackTarget, run the following command in the Package Manager Console
```
PM> Install-Package NLog.LumberjackTarget
```
###3.2. Add NLog section to your `App.config` file
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <section name="nlog" type="NLog.Config.ConfigSectionHandler, NLog"/>
  </configSections>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5.2"/>
  </startup>
  
  <nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

    <extensions>
      <add assembly="NLog.Targets.Lumberjack"/>
    </extensions>

    <targets async="true">
      <target name="MyLogstashServer" xsi:type="Lumberjack" host="<YOUR HOST NAME>" Thumbprint="<YOUR HOST CERTIFICATE THUMBPRINT WHITHOUT SPACES>"  />
    </targets>

    <rules>
      <logger name="*" minlevel="Trace" writeTo="MyLogstashServer"/>
    </rules>
    
  </nlog>
</configuration>
```
ex.: 
```
<target name="MyLogstashServer" xsi:type="Lumberjack" host="logs.myserver.com" Thumbprint="03E50C24E29F5AE39CDB83411102861828793D3F"  />
```
##4. Usage
Add namespase in your source file to use Lumberjack extesion logging methods.
```
using NLog.Targets.Lumberjack;
```
###4.1. Sending event
```
// sending log
var log = new LumberjackLogMessage("source", "application", "component", LogLevel.Info, "My info message")
{
    Tags = new HashSet<string> { "tag01", "tag02", "tag03" },
    Fields = new Dictionary<string, object> {
        { "mem", "256"},
        { "load", 0.3},
    }
};
nlog.Log(log);
```
###4.2. Sending metric
```
var message = new LumberjackMetricMessage("source", "application", "component", "auth", UnixTimeNow(), new Random().Next(50, 100))
{
    MachineName = Environment.MachineName
};
nlog.Measure(message);
```
###4.3. Sending alert
```
var alert = new LumberjackAlertMessage("source", "application", "component", "myrule", "Event raised!");
nlog.Alert(alert);
```
##5. Notes
Use debug logstash output to configure your custom [filter set](https://www.elastic.co/guide/en/logstash/current/filter-plugins.html)
```
output {
  elasticsearch { host => localhost }
  stdout { codec => rubydebug }
}
```
Run the command below to monitor logstash output
```
tail -f /var/log/logstash/logstash.stdout
```
