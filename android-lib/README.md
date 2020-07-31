LogMe library for android.

[![](https://jitpack.io/v/sontx/logme.svg)](https://jitpack.io/#sontx/logme)

Getting Started
====

To get a Git project into your build:
---

**Step 1.** Add the JitPack repository to your build file, add it in your root build.gradle at the end of repositories:

```bash
allprojects {
    repositories {
        ...
        maven { url 'https://jitpack.io' }
    }
}
```

**Step 2.** Add the dependency

```bash
dependencies {
    implementation 'com.github.sontx:logme:v2.0.4'
}
```

To use `LogMe` in your android project:
---

**Step 1.** You need a Mqtt broker server, there are many free servers such as mqtt.eclipse.org, test.mosquitto.org, broker.hivemq.com... (see more [here](https://mntolia.com/10-free-public-private-mqtt-brokers-for-testing-prototyping/))

**Step 2.** In your `MainActivity`, start new instance of `LogMe`:

```java
@Override
protected void onCreate(Bundle savedInstanceState) {
    .......
    // Create and start new LogMe instance
    this.logMe = LogMe.startNew(this, "ws://mqtt.eclipse.org:80/mqtt");
    .......
}

@Override
protected void onDestroy() {
    .......
    // Don't forget to stop LogMe instance when your app exits
    if (this.logMe != null) {
        this.logMe.stop();
    }
}
```

**Step 3.** Use `dev.sontx.logme.Logger` to write logs instead of using `android.util.Log` class

```java
Logger.i("MainActivity", "This is a simple info log");// Logger.i("This is a simple info log");
Logger.d("MainActivity", "This is a simple debug log");// Logger.i("This is a simple debug log");
Logger.e("MainActivity", "This is a simple error log", throwable);// Logger.i("This is a simple error log", throwable);
```