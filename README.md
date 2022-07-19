*Tento projekt vznikl jako ročníkový projekt a po sléze přešel do bakalářské práce na Matematicko-fyzikální fakultě Univerzity Karlovy. Navštivte [dokumentaci ročníkového projektu](https://gitlab.mff.cuni.cz/teaching/nprg045/obdrzalek/2020-21/kytka)*

# IOT Dash README
IOT Dash is an application used for managing IoT devices and displaying data collected from them.

This repository contains only the backend part. Frontend application is located in the repository [iot-dash-app](https://github.com/Muph0/iot-dash-app).

## Documentation

For detailed information, check out:
 - [REST API documentation](https://muph0.github.io/iot-dash-backend/html/rest.html)
 - [Generated class reference](https://muph0.github.io/iot-dash-backend/html/)


# Install & Run

To get download the latest release, locate a `run` script in the root of the archive and execute it.

## Requirements

The app is meant to run with the backend and broker visible to all the devices.
To run it, you will need a running instance of MySQL server and a MQTT broker.

You need to setup a MySQL user and provide credentials in configuration.

## Configure

This is a default configuration file with commentary

```jsonc
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        // This is the url the server will be listening on
        "Url": "http://*:8080"
      }
    }
  },

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

