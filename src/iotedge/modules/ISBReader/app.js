'use strict';

var express = require('express');
var bodyParser = require('body-parser');
require('isomorphic-fetch');
var Transport = require('azure-iot-device-mqtt').Mqtt;
var Client = require('azure-iot-device').ModuleClient;
var Message = require('azure-iot-device').Message;

const app = express();
const port = 3000;
const topic = process.env.TOPIC;

// Dapr publishes messages with the application/cloudevents+json content-type
app.use(bodyParser.json({ type: 'application/*+json' }));

app.get('/dapr/subscribe', (_req, res) => {
  res.json([
    topic
  ]);
});

Client.fromEnvironment(Transport, function (err, client) {
  if (err) {
    throw err;
  } else {
    client.on('error', function (err) {
      throw err;
    });

    // connect to the Edge instance
    client.open(function (err) {
      if (err) {
        throw err;
      } else {
        console.log('IoT Hub module client initialized');
      }
    });

    app.post('/' + topic, (req, res) => {
      handleServiceBusMessage(req, res, client);
    });
  }
});

function handleServiceBusMessage(request, result, client) {
  console.log("Message from Industrial Service Bus: ")
  console.log(request.body);

  if (request.body) {
    var outputMsg = new Message(JSON.stringify(request.body));
    client.sendOutputEvent('output1', outputMsg, (err, res) => {
      console.log('Sending message.');

      if (err) {
        console.log('error: ' + err.toString());
        result.sendStatus(500);
      }
      if (res) {
        console.log('status: ' + res.constructor.name);
        result.sendStatus(200);
      }
    });
  }
}

app.listen(port, () => console.log(`Node App listening on port ${port}!`));
