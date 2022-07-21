# Programmer's manual

This is an in depth manual about the architecture and individual components'
implementation of the IOT-Dash application backend.

Related:
- [README](https://github.com/Muph0/iot-dash-backend)
- [REST API and scheme documentation](rest.html)
- [REST API specification in OpenAPI format](swagger.yaml)
- [Frontend web app](https://github.com/Muph0/iot-dash-app)

[TOC]

## Get source code & build

For building, you need the
[.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) along with the
dotnet CLI.
After you pull the sources from
[the repository](https://github.com/Muph0/iot-dash-backend), get all dependencies
of the project by running

```
dotnet restore
```

# Application context
The backend is a middle man between the web application and the devices. It is
responsible for managing devices and presenting data for users of front end
application. See in the following component illustration.

```
                                                         devices
         ┌───────────────┐       ┌─────────────────┐          ┌─────┐
         │ iot-dash-app  │       │                 │◄────┬────┤     │
         └──────┬────────┘       │   MQTT Broker   │     │    ├─────┤
                │                │                 │     ├────┤     │
                │ http           └───┬─────────────┘     │    ├─────┤
                │                  ▲ │                   ├────┤     │
                │                  │ │ publish &         │    ├─────┤
                ▼                  │ │ subscribe         └────┤     │
            REST api               │ ▼                        ├─────┤
         ┌─────────────────────────┴───────────────┐          │  .  │
         │            iot-dash-backend             │          │  .  │
         └────────────────────┬────────────────────┘          │  .  │
                              │                               │     │
         ┌────────────────────┴────────────────────┐          │     │
         │              MySQL database             │          │     │
         └─────────────────────────────────────────┘          └─────┘
```

In production, the frontend webapp is hosted via backend HTTP server.
Place the built web app into the `wwwroot` directory in root of the backend repository.
Obtain it from the repository [iot-dash-app](https://github.com/Muph0/iot-dash-app).

# Conceptual model

The following objects are the data entities of the application. All functionality
is constructed around them.
Classes with relevant documentation are in the IotDash.Data.Model namespace.
Application uses Entity Framework Core to generate SQL schema from C# classes.

![Conceptual model](model.png)

# Architecture

Service-oriented architecture of the backend uses dependency injection provided by
the ASP.NET Core library. The functionality is separated into coherent groups called
*services* which exchange information through a mediator pattern
(IotDash.Services.Messaging.MessageMediator class).

During startup, services are registered by scanning the assembly for implementations
of the IotDash.Installers.IInstaller interface, which are located in the
IotDash.Installers namespace. Then the installers are instantiated and their
`Install()` methods are called.

The dependency container registers services by Type, but not directly by the service
type. The services are registered by an interface or an abstract type to allow easy
swap of implementation of different services.

## Web server pipeline

The web server uses the middleware pattern to realize the server pipeline.
Each request is passed through the pipeline call stack. The pipeline is configured in the IotDash.Startup class.

![Web server pipeline](pipeline.svg)

Different parts of the pipeline are realised by the following classes:
 - **CORS**: Microsoft.AspNetCore.Builder.CorsMiddlewareExtensions
 - **Routing**: Microsoft.AspNetCore.Routing.EndpointRoutingMiddleware
 - **Error reporting**: IotDash.Middleware.ApiErrorReporting
 - **Authentication**: Microsoft.AspNetCore.Authentication.AuthenticationMiddleware
 - **Authorization**: Microsoft.AspNetCore.Authorization.AuthorizationMiddleware
 - Different endpoints are registered by providing appropriate implementation of Microsoft.AspNetCore.Routing.IEndpointRouteBuilder to the Microsoft.AspNetCore.Routing.EndpointMiddleware on top of the pipeline.

Static files hosted by this backend application are the files of the frontend application.

### Authorization and Authentication

Authentication is realised via standard ASP.NET authenticator service with the JWT extension. The ASP.NET authenticators are configured and registered in the AuthenticationInstaller.

Authorization is realised via IotDash.Authorization.Policies registering different IotDash.Authorization.Requirements onto various controller routes.
The requirements are realised by their relevant AuthorizationHandlers.

### Controllers and Hubs

Controllers provide endpoints for HTTP routes. They are registered automatically on the <see cref="IotDash.Startup.Configure" />.
    - IotDash.Controllers.V1.IdentityController
    - IotDash.Controllers.V1.InterfaceController

Hubs provide endpoints for SignalR clients, which enable two-way real-time communication with the clients. In this application, this functionality is only used for providing real-time MQTT data to the clients.
    - IotDash.Controllers.V1.EventHub

For exact route and scheme documentation, check out the
[generated REST API reference](rest.html).

If you make any changes to the routes, get the up-to-date OpenAPI specification via
Swagger. Run the application locally in *Debug* mode and get the the specification from
the app by fetching the `GET /swagger/v1/swagger.yaml` route either by your browser or
from command line.

# Services

The most of the application's functionality is wrapped up in services
(see [Architecture](#autotoc_md3)). This section contains their complete list.

## Database context

The database context is the API of Entity Framework Core which is the ORM library this
app is using.

It is implemented by the IotDash.Data.DataContext class, which provides all database
functionality. It is registered by the IotDash.Installers.DataInstaller. It is a scoped
service, which means it is created once per HTTP request.

## Model stores

Model store classes provide abstraction over the database context. Ïnstead of directly
accessing the database, other services depend on these IotDash.Services.IModelStore
objects. They provide basic CRUD functionality over the database, plus some additional
features like change detection, user authentication etc.

These scoped services are registered in DI service container by the
IotDash.Installers.DataInstaller. The following interfaces represent the API of these services.
Open them and check their inheritance diagrams to find the relevant implementations.
    - IotDash.Services.IInterfaceStore
    - IotDash.Services.IUserStore
    - IotDash.Services.IIdentityService
    - IotDash.Services.IHistoryStore

All these stores depend on IotDash.Data.DataContext.

## Entity managers

A servise extending IotDash.Services.IEntityManagementService interface is an entity
manager. Such services are hosted and their responsibility is to monitor the database
and keep one-to-one mapping between one type of entity and their TManager, which is
a disposable service which performs some actions related to its entity.

All implementations of IotDash.Services.IEntityManagementService are installed by the
IotDash.Installers.MiscServiceInstaller.

Such managers are:
 - IotDash.Services.Evaluation.HostedEvaluationService
 - IotDash.Services.History.HostedHistoryService

They services connected to them are described in the following two sections.

### Expression evaluation

The core of expression evaluation service is the
IotDash.Services.Evaluation.HostedEvaluationService. This hosted service is installed
by IotDash.Installers.MiscServiceInstaller (see [Entity managers](#autotoc_md10)).

Classes of this service are in the IotDash.Services.Evaluation namespace.
For more information, read their class reference documentation.

The expression parsing grammar and the evaluator are defined in the IotDash.Parsing namespace. The IotDash.Parsing.Expressions namespace contains the individual expression objects. The parser and evaluators are implemented using a visitor pattern over the IotDash.Parsing.Expressions.IExpr interface.
The IotDash.Parsing.Expressions namespace contains various nodes of the syntax tree.

### History logging

History logging service is implemented by IotDash.Services.History.HostedHistoryService.
This hosted service is installed by IotDash.Installers.MiscServiceInstaller (see
[Entity managers](#autotoc_md10)).

Classes of this service are in the IotDash.Services.History namespace.
For more information, read their class reference documentation.

## MQTT Client

The MQTT Client allows the app to connect to a MQTT Broker and to delegate MQTT messages
between its components.

The MQTT client is realised via the MQTTnet library. It provides the following service
    - IotDash.Services.Mqtt.MqttMediator implemented by IotDash.Services.Mqtt.Implementation.MqttNet_Mediator

Classes of this service are the IotDash.Services.Mqtt.Implementation namespace.

## Messaging

Messaging is the communication between application services.

Messaging is realised by the mediator pattern on two levels. There is the
IotDash.Services.Messaging.MessageMediator which provides application-level messages.
For MQTT there is a separate mediator service provided by the
[MQTT Client](#autotoc_md13).

## Logging

Logging is writing information about the ongoing processes into the terminal.

Logging is provided via the ASP.NET Core application Host through the
Microsoft.Extension.Logging interfaces. The logger provider
IotDash.Services.MyConsoleLoggerProvider is registered in the host building process
all the way up at the entry point (IotDash.Program).

# Settings


