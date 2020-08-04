![Preview](https://github.com/sontx/logme/raw/master/images/send-logs.gif)

LogMe consists of Supervisor and LogMe library which helps us to see realtime logs of our app which is running in a remote device.

Getting Started
====

Supported features:

- Realtime logs.
- Gets system information of remote device which our app is running.
![](https://github.com/sontx/logme/raw/master/images/system-info.gif)
- Takes screenshot of our app.
![](https://github.com/sontx/logme/raw/master/images/screenshot.gif)
- Show unhandled exceptions.
![](https://github.com/sontx/logme/raw/master/images/unhandled-exceptions.gif)


Supervisor
---

A simple Windows application which is used to show realtime logs and more.
Download [here](https://github.com/sontx/logme/releases).

![Supervisor](https://github.com/sontx/logme/raw/master/images/supervisor.PNG)

1. There are two fields you need to fill up:
    - Server address: a mqtt broker server which will be used to subscribe published logs from our remote app. Some free servers are listed [here.](https://mntolia.com/10-free-public-private-mqtt-brokers-for-testing-prototyping/)
    - App name: which app name you want to supervise.

2. Click on **Connect** button to start supervising.
3. Click on **Screenshot** to take a screenshot of the remote app, or click on **System Info** to get system information of the remote device.

LogMe library
---

This is the client part which will be used to send logs, screenshot... to Supervisor. Currently, we only support for Android and .Net Framework.

About how to install and use this library in your project:
- [Android](https://github.com/sontx/logme/blob/master/android-lib/README.md)
- [.Net Framework](https://github.com/sontx/logme/blob/master/DotNetLib/LogMe/README.md)
