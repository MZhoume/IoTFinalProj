/// <reference path="../typings/index.d.ts" />
/// <reference path="./controllers.ts" />

interface ICallback {
    onSuccess(statCode: number, data: any): void;
    onError(statCode: number, data: any): void;
}

class StorageService {
    lastItems: IItem[];
    items: IItem[];

    isManager: boolean = true;
}

class HttpService {
    private _urlBase = 'http://0.0.0.0:5000';

    static $inject = ['$http'];
    constructor(
        private _http: angular.IHttpService
    ) {
    }

    init(urlBase: string) {
        if (urlBase.indexOf('/', urlBase.length - '/'.length) !== -1) {
            this._urlBase = urlBase.substr(0, urlBase.length - 1);
        } else {
            this._urlBase = urlBase;
        }
    }

    get(url: string, callback: ICallback) {
        this._http.get(this._urlBase + url)
            .success((d, n, h, c) => callback.onSuccess(n, d))
            .error((d, n, h, c) => callback.onError(n, d));
    }

    post(url: string, payload: any, callback: ICallback) {
        this._http.post(this._urlBase + url, payload)
            .success((d, n, h, c) => callback.onSuccess(n, d))
            .error((d, n, h, c) => callback.onError(n, d));
    }
}

angular.module('app.services', [])

    .service('StorageService', StorageService)
    .service('HttpService', HttpService);
