### Setting Up Continuous ETW Monitoring with PerfView

TODO

This command will start a PerfView recording into a 512MB memory buffer (make sure C:\Temp exists of course):

PerfView start -LogFile:C:\Temp\PerfView.log -CircularMB:512 -ThreadTime -AcceptEULA -DataFile:C:\Temp\PerfView.etl -NoView

This command will stop the PerfView recording and flush the buffer to a file:

PerfView stop -LogFile:C:\Temp\PerfView.log

PerfView has a certain performance impact, but it should be negligible because it’s not writing the logs to disk — only to a memory buffer. With that said, it will cost you 512MB physical memory (but you have 32GB so I think it should be fine). You should also monitor the system for any performance flukes for a couple of hours to make sure that it’s not causing a serious effect (which it shouldn’t).

Because 512MB is a fairly small buffer, it might only be enough for a minute or so. This is why it’s very important to stop the recording at the right moment. You will need a simple tool on the server that runs the PerfView stop command.

PerfView also has a set of automatic triggers that you could use to stop the recording. PerfView can stop collection automatically when:

* A performance counter exceeds a certain threshold
* A specific exception occurs
* A specific event log message is written
* An IIS request takes longer than a certain number of seconds to complete