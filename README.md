
# IOT Dash
IOT Dash is an application used for managing IOT devices and displaying data collected from them.

This repository contains only the backend part.

# Application architecture
The backend is a middle man between the web application and the devices.
It is responsible for managing devices and presenting data for users of front end application.

```
                                                     devices
┌───────────────┐       ┌─────────────────┐          ┌─────┬─
│ iot-dash-app  │       │                 │◄────┬────┤     │─ interfaces
└──────┬────────┘       │   MQTT Broker   │     │    ├─────┤─ ...
       │                │                 │     ├────┤     │─
       │ http           └───┬─────────────┘     │    ├─────┤─
       │                  ▲ │                   ├────┤     │─
       │                  │ │ publish &         │    ├─────┤─
       ▼                  │ │ subscribe         └────┤     │─
   REST api               │ ▼                        ├─────┤─
┌─────────────────────────┴───────────────┐          │  .  │─
│            iot-dash-backend             │          │  .  │─
└────────────────────┬────────────────────┘          │  .  │
                     │                               │     │
┌────────────────────┴────────────────────┐          │     │
│              MySQL database             │          │     │
└─────────────────────────────────────────┘          └─────┘
```

## Devices
The application offers full features only to devices specifically designed to work with it. For interfacing with generic devices, only supported operations are read and write an MQTT topic.

> A **device** is point of interaction with the internet. Typically a collection of related *interfaces* and/or MQTT topics which all correspond to one IP end point and one physical device or program.

Devices have one or more interfaces. For example: a thermometer combined with a humidity meter and a barometer are all *interfaces* managed by one *device* which reads out their measurements and publishes them to the internet.

> An **interface** is a point of interaction with physical world. A probe, sensor, switch and the like. We support only a simple value interfaces either a *probe*, that is read-only interface or a *switch*, a write-only interface.
>
> Interfaces cannot communicate with the rest of the world on their own, so that's why they are managed by a *device*.

For selected interfaces, the backend logs their state in specified intervals and keeps history logs of some density for specified amounts of time.

## User accounts
Each user can manage only devices which were registered by them.

## History logging
The app logs history of selected MQTT topics.
This history data is then presented over REST API.
For detail, see `TODO` ref REST


# Usage

## Install

The app is meant to run with the backend and broker visible to all the devices.
To run it, you will need a running instance of MySQL server and MQTT broker.

You need to setup a MySQL user and provide credentials in configuration.

## Configure

This is a default configuration file with commentary

```jsonc
{
  "ConnectionStrings": {
    // The MySQL connection string with user credentials
    "DefaultConnection": "server=localhost;port=3306;database=iot-dash-backend;uid=iot-dash;password=iot-dash"
  },

  "Jwt": {
    // The JWT secret that is used to verify and sign all issued JWT tokens
    "Secret": "kOxuIYF4o6-NN0bVnVgPcpNm0tT3-ZlZ",

    // Algorithm used for token signing
    "Algorithm": "HS256",

    // Time after which will JWT expire (format: days.hh:mm:ss)
    "TokenLifetime": "00:15:30",

    // Time for which will be expired token still accepted
    "ClockSkew": "00:00:30",

    // Time after which a refresh token validity will expire
    "RefreshTokenLifetime": "180.00:00:00"
  },

  "Mqtt": {
    "Broker": {
      // Host name of the MQTT broker
      "Host": "192.168.7.7",

      // Port on which the MQTT broker is listening
      "Port": 1883,

      // If the program fails to reconnect to broker this many times in a row
      // it will terminate with an error. -1 will never terminate.
      "MaxReconnectionAttempts": 5,
    },

    "Client": {
      "Id": "5eb020f043ba8930506acbdd"
    },
  },

  // Configuration for Microsoft logging
  "Logging": {

    "LogLevel": {
      // Log levels for different namespaces. Key is a namespace,
      // value is logging level. All messages from and above given
      // level will be logged.
      //
      // Valid levels are: 'Trace', 'Debug', 'Information',
      //                   'Warning', 'Error', 'Critical'
      // (in ascending order)
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
    }
  },
  "AllowedHosts": "*"
}

```

## Run

To run the app, currently the only option is to build it yourself via .NET CLI.
You will need .NET 5.0 SDK installed.
