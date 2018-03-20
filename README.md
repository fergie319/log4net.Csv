# log4net.Csv
CSV (Comma-Separated Values) Layout for log4net that produces properly escaped and fully valid CSV logs

# Example Configuration
The CSV Layout can be used with any log4net appender.  Following are examples for convenience.

## Rolling File Appender Example

```xml
<appender name="file" type="log4net.Appender.RollingFileAppender">
  <file value="myapplog.csv" />
  <appendToFile value="true" />
  <rollingStyle value="Size" />
  <maxSizeRollBackups value="5" />
  <maximumFileSize value="10MB" />
  <staticLogFileName value="true" />
  <layout type="log4net.Csv.CsvLayout, log4net.Csv">
    <header type="log4net.Util.PatternString" value="thread,level,class,method,message,utcdate,exception%newline" />
    <fields value="thread,level,class,method,message,utcdate,exception" />
  </layout>
</appender>
```

## Colored Console Appender Example

```xml
<appender name="console" type="log4net.Appender.ColoredConsoleAppender">
  <mapping>
    <level value="DEBUG" />
    <foreColor value="Purple" />
  </mapping>
  
  <layout type="log4net.Csv.CsvLayout, log4net.Csv">
    <fields value="thread,level,class,method,message,utcdate,exception" />
  </layout>
</appender>
```

# Supported Fields
* date
  * date formatting not supported - outputs log4net default Iso8601 format
* utcdate
  * date formatting not supported - outputs log4net default Iso8601 format
* message
* logger
* level
* class
* method
* exception
* identity
* thread
* username
* appdomain
* All custom properties are supported with a case sensitive match

