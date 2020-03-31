'use strict';

// Define app.
var root = {
    app: {
        breakpoints: {
            desktop: 840,
            height: 500,
            tablet: 480,
            phone: 0
        },
        desktop: false,
        isDebug: false,
        permissions: [],
        phone: false,
        rowData: null,
        tablet: false,
        tags: {},
        useTooltips: true,
        validationMessages: []
    },
    components: {},
    computed: {},
    dataSourceCache: {},
    dataSources: {},
    methods: {
        void: function () { }
    },
    models: {},
    page: null,
    pages: {
        configuration: {},
        reporting: {}
    },
    url: {},
    void: function () { }
};

root.buildQueryString = function (data)
{
    let args = Object.getOwnPropertyNames(data).map(function (currentValue)
    {
        return encodeURIComponent(currentValue) + '=' + encodeURIComponent(data[currentValue]);
    });
    if (args.length)
        return '?' + args.join('&');
    else
        return '';
};

root.buildUrl = function ()
{
    let args = Array.prototype.slice.call(arguments);
    let values = [];
    args.forEach(function (arg)
    {
        if (Array.isArray(arg))
            values.push.apply(values, arg);
        else if (arg !== undefined && arg !== null)
            values.push(arg);
    });
    let encodedValues = values.map(function (currentValue) { return encodeURIComponent(currentValue); });
    return root.url + (root.url.endsWith('/') ? '' : '/') + encodedValues.join('/');
};

//
// SignalR Connection State.
//
root.enums = {};
root.enums.signalRConnectionState = function (stateCode)
{
    this._stateCode = stateCode;
};
root.enums.signalRConnectionState.prototype.isConnected = function ()
{
    return this._stateCode === 1;
};