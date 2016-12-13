/// <reference path="../typings/index.d.ts" />
/// <reference path="./controllers.ts" />
var StorageService = (function () {
    function StorageService() {
        this.isManager = true;
    }
    return StorageService;
}());
var HttpService = (function () {
    function HttpService(_http) {
        this._http = _http;
        this._urlBase = 'http://0.0.0.0:5000';
    }
    HttpService.prototype.init = function (urlBase) {
        if (urlBase.indexOf('/', urlBase.length - '/'.length) !== -1) {
            this._urlBase = urlBase.substr(0, urlBase.length - 1);
        }
        else {
            this._urlBase = urlBase;
        }
    };
    HttpService.prototype.get = function (url, callback) {
        this._http.get(this._urlBase + url)
            .success(function (d, n, h, c) { return callback.onSuccess(n, d); })
            .error(function (d, n, h, c) { return callback.onError(n, d); });
    };
    HttpService.prototype.post = function (url, payload, callback) {
        this._http.post(this._urlBase + url, payload)
            .success(function (d, n, h, c) { return callback.onSuccess(n, d); })
            .error(function (d, n, h, c) { return callback.onError(n, d); });
    };
    HttpService.$inject = ['$http'];
    return HttpService;
}());
angular.module('app.services', [])
    .service('StorageService', StorageService)
    .service('HttpService', HttpService);

/// <reference path="../typings/index.d.ts" />
/// <reference path="./services.ts" />
var WelcomeCtrl = (function () {
    function WelcomeCtrl(_scope, _httpSvc) {
        this._scope = _scope;
        this._httpSvc = _httpSvc;
        _scope.initIpAddr = function (ip) {
            _httpSvc.init(ip);
        };
    }
    WelcomeCtrl.$inject = ['$scope', 'HttpService'];
    return WelcomeCtrl;
}());
var ItemsCtrl = (function () {
    function ItemsCtrl(_scope, _httpSvc, _storageSvc) {
        this._scope = _scope;
        this._httpSvc = _httpSvc;
        this._storageSvc = _storageSvc;
        _scope.isManager = _storageSvc.isManager;
        this.getItems();
    }
    ItemsCtrl.prototype.getItems = function () {
        var _this = this;
        this._httpSvc.get('/info', {
            onSuccess: function (c, d) {
                _this._scope.hasError = false;
                _this._storageSvc.lastItems = _this._storageSvc.items;
                _this._scope.items = d;
                _this._storageSvc.items = d;
            },
            onError: function (c, d) {
                _this._scope.hasError = true;
                _this._scope.error = d;
            }
        });
        setTimeout(function () {
            _this.getItems();
        }, 3000);
    };
    ItemsCtrl.$inject = ['$scope', 'HttpService', 'StorageService'];
    return ItemsCtrl;
}());
var ItemsChangeCtrl = (function () {
    function ItemsChangeCtrl(_scope, _params, _httpSvc, _storageSvc) {
        var _this = this;
        this._scope = _scope;
        this._params = _params;
        this._httpSvc = _httpSvc;
        this._storageSvc = _storageSvc;
        _scope.item = _.find(_storageSvc.items, function (i) { return i.id === Number(_params.id); });
        _scope.submitHumidity = function (humidity) {
            _this._httpSvc.get('/adjust/' + _params.id.toString() + '/humidity?h=' + humidity.toString(), {
                onSuccess: function (c, d) {
                    _this._scope.hasError = false;
                    _this._scope.hasResult = true;
                    _this._scope.result = d;
                },
                onError: function (c, d) {
                    _this._scope.hasResult = false;
                    _this._scope.hasError = true;
                    _this._scope.error = d;
                }
            });
        };
        _scope.submitTemperature = function (temperature) {
            _this._httpSvc.get('/adjust/' + _params.id.toString() + '/temperature?t=' + temperature.toString(), {
                onSuccess: function (c, d) {
                    _this._scope.hasError = false;
                    _this._scope.hasResult = true;
                    _this._scope.result = d;
                },
                onError: function (c, d) {
                    _this._scope.hasResult = false;
                    _this._scope.hasError = true;
                    _this._scope.error = d;
                }
            });
        };
        _scope.submitPrice = function (price) {
            _this._httpSvc.get('/price/' + _params.id.toString() + '/set?p=' + price.toString(), {
                onSuccess: function (c, d) {
                    _this._scope.hasError = false;
                    _this._scope.hasResult = true;
                    _this._scope.result = d;
                },
                onError: function (c, d) {
                    _this._scope.hasResult = false;
                    _this._scope.hasError = true;
                    _this._scope.error = d;
                }
            });
        };
        _scope.refillItem = function () {
            _this._httpSvc.get('/refill/' + _params.id.toString(), {
                onSuccess: function (c, d) {
                    _this._scope.hasError = false;
                    _this._scope.hasResult = true;
                    _this._scope.result = d;
                },
                onError: function (c, d) {
                    _this._scope.hasResult = false;
                    _this._scope.hasError = true;
                    _this._scope.error = d;
                }
            });
        };
    }
    ItemsChangeCtrl.$inject = ['$scope', '$stateParams', 'HttpService', 'StorageService'];
    return ItemsChangeCtrl;
}());
var MiscCtrl = (function () {
    function MiscCtrl(_scope, _httpSvc, _storageSvc) {
        this._scope = _scope;
        this._httpSvc = _httpSvc;
        this._storageSvc = _storageSvc;
        _scope.settings = _storageSvc;
        _httpSvc.get('/plan', {
            onSuccess: function (c, d) {
                _scope.misc = d;
            },
            onError: function (c, d) {
                _scope.misc = d;
            }
        });
    }
    MiscCtrl.$inject = ['$scope', 'HttpService', 'StorageService'];
    return MiscCtrl;
}());
angular.module('app.controllers', [])
    .controller('WelcomeCtrl', WelcomeCtrl)
    .controller('ItemsCtrl', ItemsCtrl)
    .controller('ItemsChangeCtrl', ItemsChangeCtrl)
    .controller('MiscCtrl', MiscCtrl);
