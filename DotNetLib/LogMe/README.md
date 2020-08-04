LogMe library for .Net Framework.

[![](https://jitpack.io/v/sontx/logme.svg)](https://jitpack.io/#sontx/logme)

Getting Started
====

To instsall LogMe in your project:
---

Install from Package Manager:
```bash
Install-Package LogMeLib -Version 1.0.0
```

Or install from .Net CLI
```bash
dotnet add package LogMeLib --version 1.0.0
```

To use `LogMe` in your .Net Framework project:
---

**Step 1.** You need a Mqtt broker server, there are many free servers such as mqtt.eclipse.org, test.mosquitto.org, broker.hivemq.com... (see more [here](https://mntolia.com/10-free-public-private-mqtt-brokers-for-testing-prototyping/))

**Step 2.** In your `Main` method or main window, start new instance of `LogMe`:

```cs
static LogMe logMe;

static void Main()
{
    StartLogMe();
    ...................
    StopLogMe();
}

static async void StartLogMe()
{
    logMe = new LogMe("ws://mqtt.eclipse.org:80/mqtt");
    await logMe.StartAsync();
}

static async void StopLogMe()
{
    await logMe.StopAsync();
}
```

**Step 3.** Use `LogMeLib.Logger` to write logs.

```java
Logger.I("MainActivity", "This is a simple info log");// Logger.i("This is a simple info log");
Logger.D("MainActivity", "This is a simple debug log");// Logger.i("This is a simple debug log");
Logger.E("MainActivity", "This is a simple error log", throwable);// Logger.i("This is a simple error log", throwable);
```