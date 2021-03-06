﻿declare var $: any;
declare var Handlebars: any;
declare var crossroads: any;
declare var hasher: any;

declare module Hadouken.Events {
    class EventListener {
        addHandler(name: string, callback: { (data: any): void; }): void;
        clearHandlers(): void;
        connect(): void;
        disconnect(): void;
        sendEvent(name: string, data: any): void;
    }
}

declare module Hadouken.UI {
    class Page {
        content: any;

        constructor(url: string, routes: Array<string>);
        init(): void;
        load(): void;
    }

    class PageManager {
        constructor();
        public static getInstance(): PageManager;
        addPage(page: any): void;
    }

    class Dialog {
        constructor(url: string);
        show();
        close();
        onShow();

        getContent(): any;
    }

    class WizardStep {
        name: string;
        content: any;

        constructor(url: string, name: string);

        public load(callback: any): void;

        public onloaded(): void;

        public saveData(callback: any): void;

        public loadData(callback: any): void;
    }
}

declare module Hadouken.Plugins {
    class Plugin {
        load(): void;
        unload(): void;
        configure(): void;
        initialConfiguration(): Hadouken.UI.WizardStep;
    }

    class PluginEngine {
        public static getInstance(): PluginEngine;
        public setPlugin(id: string, plugin: Plugin): void;
    }
}

declare module Hadouken.Http {
    class JsonRpcClient {
        constructor(url: string);
        call(method: string, callback: { (result: any): void; }): void;
        callParams(method: string, params: any, callback: { (result: any): void; }): void;
    }
}