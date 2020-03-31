'use strict';

root.components.connectionStatus = function ()
{
    this.$topappbar = null;
    this.everConnected = false;
    this.onConnected = null;
    this.onDisconnected = null;
    this.showConnectedMessage = false;
    this.simulation = false;
    this.simulationDisplayingColor = false;
    this.simulationDisplayingText = false;
    this.state = this.STATE_INIT;
    this.stateText = null;
    this.tagReadsWaitingCounts = {};
    this.toast = null;
    this.validationCounts = null;
    this.isInError = false;
};

root.components.connectionStatus.prototype.connecting = function (message)
{
    console.warn(message);
    this.message = this.everConnected ? 'Reconnecting...' : message;
    this.state = this.STATE_CONNECTING;
};

root.components.connectionStatus.prototype.connected = function ()
{
    console.log('Connected.');
    this.everConnected = true;
    this.message = null;
    this.state = this.STATE_CONNECTED;

    if (typeof this.onConnected === 'function') this.onConnected();
};

root.components.connectionStatus.prototype.disconnected = function (message)
{
    console.warn(message);
    this.message = message;
    this.state = this.STATE_DISCONNECTED;

    if (typeof this.onDisconnected === 'function') this.onDisconnected();
};

root.components.connectionStatus.prototype.hideMessage = function ()
{
    if (this.toast)
    {
        this.toast.hide();
    }
};

root.components.connectionStatus.prototype.performErrorCheck = function ()
{
    let indicator = null;
    let message = null;
    let that = this;

    //
    // Message Banner.
    //
    
    // Should we show a message?
    if (this.state === this.STATE_DISCONNECTED)
    {
        this.isInError = true;
        indicator = this.INDICATOR_RED;
        message = this.message || 'Disconnected.';
    }
    else if (this.state === this.STATE_CONNECTING)
    {
        this.isInError = true;
        indicator = this.INDICATOR_RED;
        message = this.message || 'Connecting...';
    }
    else if (this.state === this.STATE_CONNECTED && this.showConnectedMessage)
    {
        this.isInError = false;
        this.showConnectedMessage = false;
        indicator = this.INDICATOR_GREEN;
        message = this.message || 'Connected.';
    }
    else
    {
        this.isInError = false;
        let totalCount = Object.keys(this.tagReadsWaitingCounts).reduce(function (accumulator, property)
        {
            let count = that.tagReadsWaitingCounts[property] || 0;
            return accumulator + count;
        }, 0);
        if (totalCount > 0)
        {
            indicator = this.INDICATOR_RED;
            message = 'Waiting to read ' + totalCount + ' records...';
        }
    }

    // Show message?
    if (indicator && message)
    {
        this.showMessage(indicator, message);
    }
    else if (this.toast && this.toast.option('visible'))
    {
        this.hideMessage();
    }


    //
    // Simulation.
    //

    // Lookup $topappbar.
    if (!this.$topappbar || !this.$topappbar.length)
    {
        this.$topappbar = $('.mdc-top-app-bar');
    }

    // Show / hide color.
    if (this.simulation && !this.simulationDisplayingColor)
    {
        this.$topappbar.addClass('simulation-color');
        this.simulationDisplayingColor = true;
    }
    else if (!this.simulation && this.simulationDisplayingColor)
    {
        this.$topappbar.removeClass('simulation-color');
        this.simulationDisplayingColor = false;
    }

    // Show / hide text.
    if (this.simulation && !this.simulationDisplayingText)
    {
        this.$topappbar.addClass('simulation-text');
        this.simulationDisplayingText = true;
    }
    else if (!this.simulation && this.simulationDisplayingText)
    {
        this.$topappbar.removeClass('simulation-text');
        this.simulationDisplayingText = false;
    }


    //
    // Validation counts.
    //
    if (this.validationCounts)
    {
        // Overview.
        if ((this.validationCounts.find(function (c) { return c.key === 'Overview'; }) || {}).value || 0)
            $('#overview').addClass('has-error');
        else
            $('#overview').removeClass('has-error');

        // Reporting.
        if ((this.validationCounts.find(function (c) { return c.key === 'Reporting'; }) || {}).value || 0)
            $('#reporting').addClass('has-error');
        else
            $('#reporting').removeClass('has-error');

        // Configuration.
        if ((this.validationCounts.find(function (c) { return c.key === 'Configuration'; }) || {}).value || 0)
            $('#configuration').addClass('has-error');
        else
            $('#configuration').removeClass('has-error');
    }
};

root.components.connectionStatus.prototype.showMessage = function (indicator, message)
{
    console.warn('Toast', indicator, message);
};

root.components.connectionStatus.prototype.startTimer = function ()
{
    setInterval(this.performErrorCheck.bind(this), this.ERROR_CHECK_INTERVAL_IN_MILLISECONDS);
};

root.components.connectionStatus.prototype.updateSimulation = function (tagChange)
{
    if (tagChange && tagChange.good && typeof tagChange.tagValue === 'boolean')
    {
        this.simulation = tagChange.tagValue;
    }
};

root.components.connectionStatus.prototype.updateTagReadsWaitingCount = function (count, tagGroup)
{
    if (typeof count === 'number' && typeof tagGroup === 'string')
    {
        this.tagReadsWaitingCounts[tagGroup] = count;
    }
};

root.components.connectionStatus.prototype.updateValidationCounts = function (dictionary)
{
    if (dictionary)
    {
        this.validationCounts = dictionary;
    }
};

root.components.connectionStatus.prototype.ERROR_CHECK_INTERVAL_IN_MILLISECONDS = 1000;
root.components.connectionStatus.prototype.INDICATOR_GREEN = 'success';
root.components.connectionStatus.prototype.INDICATOR_YELLOW = 'warning';
root.components.connectionStatus.prototype.INDICATOR_RED = 'error';
root.components.connectionStatus.prototype.STATE_INIT = 0;
root.components.connectionStatus.prototype.STATE_CONNECTING = 1;
root.components.connectionStatus.prototype.STATE_CONNECTED = 2;
root.components.connectionStatus.prototype.STATE_DISCONNECTED = 3;