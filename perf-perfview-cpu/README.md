### Diagnosing High CPU Utilization with PerfView

In this lab, you will use PerfView to profile the CPU utilization and overall performance of a .NET application.

#### Task 1

Run the StupidNotepad.exe application from the [bin](bin/) folder and start typing some text into the edit box. You'll notice that the UI occasionally stutters and stops responding for short periods of time. If you open a Task Manager window and inspect the CPU utilization at the same time, you will also notice CPU usage spikes when typing into the application.

#### Task 2

Run PerfView.exe and select **Collect** > **Run** from the menu. Specify the full path to the StupidNotepad.exe application in the [bin](bin/) folder. Make sure to check the *Thread Time* checkbox before clicking **Run**.

When the application opens, type some text again. Make sure the UI hiccup occurs multiple times, and then close the application and go back to PerfView. In PerfView, double-click the generated .etl file and choose the *Thread Time (with Tasks) Stacks* report.

> The **Thread Time (with Tasks) Stacks** report provides an accounting of where the application spends its time, and understands relationships between a thread that launched a TPL task and a thread that executed that task. Specifically, if a thread waits for a task to complete, the task's execution time and location (stack) are attributed to the the waiting thread.

In the resulting window, go to the **CallTree** tab and expand the StupidNotepad process and the main (startup) thread. Click the checkboxes to expand the view until you reach the `MainForm.mainText_KeyPress` event handler, which spends quite a bit of CPU time and waits for a task.
 
![Screenshot of PerfView with the right call stack expanded](figure1.png)

> If you see frames with unresolved symbols (?!?), right-click the frame and select **Lookup Symbols** to resolve them. You can also experiment with command line shortcuts: Alt+S to lookup symbols, Alt+M to fold, and so forth.
