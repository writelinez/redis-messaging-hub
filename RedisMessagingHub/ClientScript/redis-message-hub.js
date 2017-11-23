var RedisMessageHub = (function () {
    //url, useSsl
    return function (options) {

        var _ws = undefined;
        var _url = undefined;

        var _authData = undefined;
        var _onConnected = undefined;
        var _onMessage = undefined;
        var _onError = undefined;
        var _onClosed = undefined;
        var _onChannelJoined = undefined;
        var _onChannelRetired = undefined;

        var _ws_onopen = null;
        var _ws_onmessage = null;
        var _ws_onclose = null;
        var _ws_onerror = null;

        //CTR
        (function () {
            var wsType = 'ws://'
            if (!options || !options.url) {
                throw new RedisMessageHubException('Argument Exception', 'RedisMessageHub requires options in the constructor. url is required.');
            }
            if (options.useSsl) {
                wsType = 'wss://'
            }
            _url = wsType + options.url;
        })();



        //WEBSOCKET FUNCTIONS
        _ws_onopen = function (data) {
            if (_ws.readyState == WebSocket.OPEN) {
                if (_authData) {
                    var strMsg = new HubMessage(0, MESSAGE_TYPE.AUTHENTICATE, '', _authData).toString();
                    if (_ws.readyState == WebSocket.OPEN) {
                        _ws.send(strMsg);
                    }
                }
                if (_onConnected) {
                    _onConnected();
                }
            }
        };

        _ws_onmessage = function (data) {
            if (_ws.readyState == WebSocket.OPEN) {

                var msg = HubMessage.fromString(data.data);

                switch (msg.type) {
                    case MESSAGE_TYPE.PING_PONG:
                        msg.data = 'PONG';
                        msg.date = new Date();
                        if (_ws.readyState == WebSocket.OPEN) {
                            _ws.send(msg.toString());
                        }
                        break;
                    case MESSAGE_TYPE.MESSAGE:
                        if (_onMessage)
                            _onMessage(msg);
                        break;
                    case MESSAGE_TYPE.SUBSCRIBE_CONFIRM:
                        if (_onChannelJoined)
                            _onChannelJoined(msg);
                        break;
                    case MESSAGE_TYPE.UNSUBSCRIBE_CONFIRM:
                        if (_onChannelRetired) {
                            _onChannelRetired(msg);
                        }
                        break;
                }
            }
        };

        _ws_onclose = function (data) {
            if (_onClosed) {
                _onClosed(data);
            }
        };

        _ws_onerror = function (data) {
            if (_onError) {
                _onError(data);
            }
        };




        //UTILITY FUNCTIONS
        function HubMessage(id, type, channel, data, date) {
            var msg = {
                Id: id,
                Type: type,
                Channel: channel,
                Data: data,
                Date: date ? date : new Date()
            }

            this.id = msg.Id;
            this.type = msg.Type;
            this.channel = msg.Channel;
            this.data = msg.Data;
            this.date = msg.Date;
            this.toString = function () {
                msg.Id = this.id;
                msg.Type = this.type;
                msg.Channel = this.channel;
                msg.Data = this.data;
                msg.Date = this.date;
                return JSON.stringify(msg);
            }
        }
        HubMessage.fromString = function (str) {
            var msg = JSON.parse(str);
            return new HubMessage(msg.Id, msg.Type, msg.Channel, msg.Data, msg.Date);
        }

        function RedisMessageHubException(type, value) {
            this.type = type;
            this.value = value;
            this.toString = function () {
                return this.type + this.value
            }
        }

        function guid() {
            return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
                (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
            )
        }



        //ENUMS
        function MESSAGE_TYPE() { };
        MESSAGE_TYPE.PING_PONG = 0;
        MESSAGE_TYPE.MESSAGE = 1;
        MESSAGE_TYPE.SUBSCRIBE = 2;
        MESSAGE_TYPE.UNSUBSCRIBE = 3;
        MESSAGE_TYPE.AUTHENTICATE = 4;
        MESSAGE_TYPE.SUBSCRIBE_CONFIRM = 5,
        MESSAGE_TYPE.UNSUBSCRIBE_CONFIRM = 6;
        MESSAGE_TYPE.INSTANCEID_REQUEST = 7;

        function HUB_STATE() { };
        HUB_STATE.DISCONNECTED = 0;
        HUB_STATE.CONNECTED = 1;










        return {
            //PUBLIC ENUMS
            MESSAGE_TYPE: MESSAGE_TYPE,
            HUB_STATE: HUB_STATE,


            //PUBLIC FUNCTIONS
            // authData, onConnected(), onMessage(message), onError(data), onClosed(data), onChannelJoined(message), onChannelRetired(message)
            connect: function (connectionOptions) {

                if (connectionOptions) {
                    _authData = connectionOptions.authData;
                    _onConnected = connectionOptions.onConnected;
                    _onMessage = connectionOptions.onMessage;
                    _onError = connectionOptions.onError;
                    _onClosed = connectionOptions.onClosed;
                    _onChannelJoined = connectionOptions.onChannelJoined;
                    _onChannelRetired = connectionOptions.onChannelRetired;
                }

                _ws = new WebSocket(_url);
                _ws.onopen = _ws_onopen;
                _ws.onmessage = _ws_onmessage;
                _ws.onclose = _ws_onclose;
                _ws.onerror = _ws_onerror;
            },

            disconnect: function () {
                if (_ws) {
                    _ws.close();
                    _ws = undefined;
                }
            },

            state: function () {
                if (_ws && _ws.readyState == WebSocket.OPEN)
                    return HUB_STATE.CONNECTED;
                else
                    return HUB_STATE.DISCONNECTED;
            },

            joinChannel: function (channel) {
                if (_ws && _ws.readyState == WebSocket.OPEN && channel) {
                    var strMsg = new HubMessage(0, MESSAGE_TYPE.SUBSCRIBE, channel, '', new Date()).toString();
                    _ws.send(strMsg);
                }
            },

            leaveChannel: function (channel) {
                if (_ws && _ws.readyState == WebSocket.OPEN && channel) {
                    var strMsg = new HubMessage(0, MESSAGE_TYPE.UNSUBSCRIBE, channel, '', new Date()).toString();
                    _ws.send(strMsg);
                }
            },

            sendMessage: function (channel, msg) {
                if (_ws && _ws.readyState == WebSocket.OPEN && channel) {
                    var strMsg = new HubMessage(0, MESSAGE_TYPE.MESSAGE, channel, msg, new Date()).toString();
                    _ws.send(strMsg);
                }
            }
        }
    }
})();