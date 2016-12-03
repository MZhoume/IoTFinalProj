/// <reference path="../typings/index.d.ts" />
/// <reference path="./services.ts" />

interface IItem {
    itemId: number;
    name: string;
    weight: number;
    humidity: number;
    temperature: number;
    price: number;
}

interface IWelcomeScope extends angular.IScope {
    initIpAddr(ipAddr: string): void;
}

class WelcomeCtrl {
    static $inject = ['$scope', 'HttpService'];
    constructor(
        private _scope: IWelcomeScope,
        private _httpSvc: HttpService
    ) {
        _scope.initIpAddr = (ip) => {
            _httpSvc.init(ip);
        }
    }
}

interface IItemsScope extends angular.IScope {
    items: IItem[];

    isManager: boolean;

    hasError: boolean;
    error: string;
}

class ItemsCtrl {
    static $inject = ['$scope', 'HttpService', 'StorageService'];
    constructor(
        private _scope: IItemsScope,
        private _httpSvc: HttpService,
        private _storageSvc: StorageService
    ) {
        _scope.isManager = _storageSvc.isManager;
        this.getItems();
    }

    getItems() {
        this._httpSvc.get('/info', {
            onSuccess: (c, d) => {
                this._scope.hasError = false;
                this._storageSvc.lastItems = this._storageSvc.items;
                this._scope.items = d;
                this._storageSvc.items = d;
            },

            onError: (c, d) => {
                this._scope.hasError = true;
                this._scope.error = d;
            }
        });
        setTimeout(() => {
            this.getItems();
        }, 1000);
    }
}

interface IItemsChangeScope extends angular.IScope {
    item: IItem;
    submitHumidity(humidity: number): void;
    submitTemperature(temperature: number): void;
    submitPrice(price: number): void;

    hasResult: boolean;
    result: string;

    hasError: boolean;
    error: string;
}

class ItemsChangeCtrl {
    static $inject = ['$scope', '$stateParams', 'HttpService', 'StorageService'];
    constructor(
        private _scope: IItemsChangeScope,
        private _params: { id: string },
        private _httpSvc: HttpService,
        private _storageSvc: StorageService
    ) {
        _scope.item = _.find(_storageSvc.items, (i) => i.itemId === Number.parseInt(_params.id));

        _scope.submitHumidity = (humidity) => {
            this._httpSvc.get('/adjust/' + _params.id.toString() + '/humidity?h=' + humidity.toString(),
                {
                    onSuccess: (c, d) => {
                        this._scope.hasError = false;
                        this._scope.hasResult = true;
                        this._scope.result = d;
                    },

                    onError: (c, d) => {
                        this._scope.hasResult = false;
                        this._scope.hasError = true;
                        this._scope.error = d;
                    }
                });
        };

        _scope.submitTemperature = (temperature) => {
            this._httpSvc.get('/adjust/' + _params.id.toString() + '/temperature?t=' + temperature.toString(),
                {
                    onSuccess: (c, d) => {
                        this._scope.hasError = false;
                        this._scope.hasResult = true;
                        this._scope.result = d;
                    },

                    onError: (c, d) => {
                        this._scope.hasResult = false;
                        this._scope.hasError = true;
                        this._scope.error = d;
                    }
                });
        };

        _scope.submitPrice = (price) => {
            this._httpSvc.get('/price/' + _params.id.toString() + '/set?p=' + price.toString(),
                {
                    onSuccess: (c, d) => {
                        this._scope.hasError = false;
                        this._scope.hasResult = true;
                        this._scope.result = d;
                    },

                    onError: (c, d) => {
                        this._scope.hasResult = false;
                        this._scope.hasError = true;
                        this._scope.error = d;
                    }
                });
        };
    }
}

interface IMiscScope extends angular.IScope {
    misc: string;
}

class MiscCtrl {
    static $inject = ['$scope', 'HttpService'];
    constructor (
        private _scope: IMiscScope,
        private _httpSvc: HttpService
    ) {
        _httpSvc.get('/plan', {
            onSuccess: (c, d) => {
                _scope.misc = d;
            },
            onError: (c, d) => {
                _scope.misc = d;
            }
        })
    }
}

angular.module('app.controllers', [])

    .controller('WelcomeCtrl', WelcomeCtrl)
    .controller('ItemsCtrl', ItemsCtrl)
    .controller('ItemsChangeCtrl', ItemsChangeCtrl)
    .controller('MiscCtrl', MiscCtrl);
