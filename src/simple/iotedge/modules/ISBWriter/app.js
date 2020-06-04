'use strict';

var express = require('express');
var request = require('request');
var bodyParser = require('body-parser');
var Transport = require('azure-iot-device-mqtt').Mqtt;
var Client = require('azure-iot-device').ModuleClient;

const app = express();
app.use(bodyParser.json());

const daprPort = process.env.DAPR_HTTP_PORT || 3500;
const daprUrl = `http://localhost:${daprPort}/v1.0`;
const publishUrl = `${daprUrl}/publish/${process.env.TOPIC}`;

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

        // Act on input messages to the module.
        client.on('inputMessage', function (inputName, msg) {
          pipeMessage(client, inputName, msg);
        });
      }
    });
  }
});

// This function just pipes the messages without any change.
function pipeMessage(client, inputName, msg) {
  if (inputName === 'input1') {
    var message = msg.getBytes().toString('utf8');
    if (message) {
      request({ uri: publishUrl, method: 'POST', json: message }, (error, response, body) => {
        if (error) {
          console.log('Error:' + error);
        }
      });
    }
  }

  client.complete(msg, (err, res) => {
    console.log('Receiving message.');
    if (err) {
      console.log('error: ' + err.toString());
    }
    if (res) {
      console.log('status: ' + res.constructor.name);
    }
  });
}
