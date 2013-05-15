/*
Copyright (c) 2011 BitTorrent, Inc. All rights reserved.

Use of this source code is governed by a BSD-style that can be
found in the LICENSE file.
*/
var g_winTitle = "Hadouken v" + CONST.VERSION;

// Localized string globals ... initialized in loadLangStrings()

var g_perSec; // string representing "/s"
var g_dayCodes; // array of strings representing ["Mon", "Tue", ..., "Sun"]
var g_dayNames; // array of strings representing ["Monday", "Tuesday", ... , "Sunday"]
var g_schLgndEx; // object whose values are string explanations of scheduler table colors

// Pre-generated elements

var ELE_TD = new Element("td");
var ELE_TR = new Element("tr");

var linkedEvent = Browser.ie ? "click" : "change";

//================================================================================
// MAIN
//================================================================================

function do_webui_main() {
    $(document.body);
    setupGlobalEvents();
    setupUserInterface();
    utWebUI.init();
}
if (!window.raptor) {
    window.addEvent("domready", function() {
        do_webui_main();    
    });
}

function setupWindowEvents() {
    //--------------------------------------------------
    // WINDOW EVENTS
    //--------------------------------------------------

    window.addEvent("resize", resizeUI);

    if (!isGuest) {
        window.addEvent("unload", function() {
            utWebUI.saveConfig(false);
        });
    }
}
function setupDocumentEvents() {
    //--------------------------------------------------
    // DOCUMENT EVENTS
    //--------------------------------------------------

    if (!isGuest) {
        document.addStopEvents({
            "dragenter": null,
            "dragover": null,
            "drop": function(ev) {
                var dt = ev.event.dataTransfer;
                if (!dt) return;

                var data;

                if ((data = dt.getData("Text"))) {
                    // Text/URL dropped
                    data = data.split(/[\r\n]+/g).map(String.trim);
                    utWebUI.addURL({url: data});
                }

                if ((data = dt.files) && data.length > 0) {
                    // Files dropped
                    utWebUI.addFile({file: data});
                }
            }
        });
    }
}

function setupMouseEvents() {
    //--------------------------------------------------
    // MOUSE EVENTS
    //--------------------------------------------------

    var mouseWhitelist = function(ev) {
        var targ = ev.target, tag = targ.tagName.toLowerCase();
        return (
            targ.retrieve("mousewhitelist") ||
            ("textarea" === tag) ||
            (("input" === tag) && !targ.disabled && ["text", "file", "password"].contains(targ.type.toLowerCase())) ||
            (("select" === tag) && !ev.isRightClick())
        );
    };
    var mouseWhitelistWrap = function(ev) {
        return !ev.isRightClick() || mouseWhitelist(ev);
    };

    // -- Select

    window.addStopEvent("mousedown", mouseWhitelist);

    // -- Right-click

    document.addStopEvents({
        "mousedown": function(ev) {
            ContextMenu.hide();
            return mouseWhitelistWrap(ev);
        },
        "contextmenu": mouseWhitelist, // IE does not send right-click info for onContextMenu
        "mouseup": mouseWhitelistWrap,
        "click": mouseWhitelistWrap
    });

    if (Browser.opera && !("oncontextmenu" in document.createElement("foo"))) {

        // Prevent Opera context menu from showing
        // - http://my.opera.com/community/forums/findpost.pl?id=2112305
        // - http://dev.fckeditor.net/changeset/683

        var overrideButton;
        document.addEvents({
            "mousedown": function(ev) {
                if (!overrideButton && ev.isRightClick()) {
                    var doc = ev.target.ownerDocument;
                    overrideButton = doc.createElement("input");
                    overrideButton.type = "button";
                    overrideButton.style.cssText = "z-index:1000;position:fixed;top:" + (ev.client.y - 2) + "px;left:" + (ev.client.x - 2) + "px;width:5px;height:5px;opacity:0.01";
                    (doc.body || doc.documentElement).appendChild(overrideButton);
                }
            },
            "mouseup": function(ev) {
                if (overrideButton) {
                    overrideButton.destroy();
                    overrideButton = undefined;
                }
            }
        });
    }

}

function setupKeyboardEvents() {
    //--------------------------------------------------
    // KEYBOARD EVENTS
    //--------------------------------------------------

    if (!isGuest) {
        var keyBindings = {
            "ctrl a": Function.from(),
            "ctrl e": Function.from(),

            "ctrl o": utWebUI.showAddTorrent.bind(utWebUI),
            "ctrl p": utWebUI.showSettings.bind(utWebUI),
            "ctrl u": utWebUI.showAddURL.bind(utWebUI),
            "f2": utWebUI.showAbout.bind(utWebUI),

            "f4": utWebUI.toggleToolbar.bind(utWebUI),
            "f6": utWebUI.toggleDetPanel.bind(utWebUI),
            "f7": utWebUI.toggleCatPanel.bind(utWebUI),

            "esc": function() {
                if (!ContextMenu.hidden) {
                    ContextMenu.hide();
                } else if (DialogManager.showing.length > 0) {
                    DialogManager.hideTopMost(true);
                } else {
                    utWebUI.restoreUI();
                }
            }
        };

        var keyBindingModalOK = {
            "esc": 1
        };

        if (Browser.Platform.mac) {
            keyBindings["meta a"] = keyBindings["ctrl a"];
            keyBindings["meta e"] = keyBindings["ctrl e"];
            keyBindings["meta o"] = keyBindings["ctrl o"];
            keyBindings["meta p"] = keyBindings["ctrl p"];
            keyBindings["meta u"] = keyBindings["ctrl u"];

            delete keyBindings["ctrl a"];
            delete keyBindings["ctrl e"];
            delete keyBindings["ctrl o"];
            delete keyBindings["ctrl p"];
            delete keyBindings["ctrl u"];
        }

        document.addStopEvent("keydown", function(ev) {
            var key = eventToKey(ev);
            if (keyBindings[key]) {
                if (!DialogManager.modalIsVisible() || keyBindingModalOK[key])
                    keyBindings[key]();
            }
            else {
                return true;
            }
        });

        if (Browser.opera) {
            document.addEvent("keypress", function(ev) {
                return !keyBindings[eventToKey(ev)];
            });
        }
    }
}

//================================================================================
// GLOBAL EVENT SETUP
//================================================================================

var __executed_setupGlobalEvents__;

function setupGlobalEvents() {

    if (__executed_setupGlobalEvents__) return;
    __executed_setupGlobalEvents__ = true;

    ContextMenu.init("ContextMenu");

    setupWindowEvents();

    setupDocumentEvents();

    setupMouseEvents();

    setupKeyboardEvents();

}

var __resizeUI_ready__ = false;

function resizeUI(hDiv, vDiv) {

    if (!__resizeUI_ready__) return;
    __resizeUI_ready__ = false;

    if (!ContextMenu.hidden)
        ContextMenu.hide();

    var manualH = (typeOf(hDiv) == 'number'),
        manualV = (typeOf(vDiv) == 'number');

    var size = window.getZoomSize(), ww = size.x, wh = size.y;

    var config = utWebUI.config || utWebUI.defConfig,
        uiLimits = utWebUI.limits,
        minHSplit = uiLimits.minHSplit,
        minVSplit = uiLimits.minVSplit,
        minTrtH = uiLimits.minTrtH,
        minTrtW = uiLimits.minTrtW;

    var badIE = (Browser.ie && Browser.version <= 6);
    var showCat = config.showCategories,
        showDet = config.showDetails,
        showSB = config.showStatusBar,
        showTB, tallCat;

    if (!isGuest) {
        showTB = config.showToolbar;
        tallCat = !!utWebUI.settings["gui.tall_category_list"];
    }

    var eleTB = $("mainToolbar");
    var tbh = (showTB ? eleTB.getHeight() : 0);
    if (showTB) {
        // Show/hide toolbar chevron
        var eleTBChildren = eleTB.getElements(".tbbutton");
        var showTBChev = false;

        for (var i = eleTBChildren.length - 1; i >= 0; --i) {
            if (eleTBChildren[i].getPosition().y > tbh) {
                showTBChev = true;
                break;
            }
        }

        if (showTBChev) {
            $("tbchevron").show();
        }
        else {
            $("tbchevron").hide();
        }
    }

    var sbh = (showSB ? $("mainStatusBar").getHeight() : 0);

    if (manualH) {
        hDiv -= 2;

        // Sanity check manual drag of divider
        if (hDiv < minHSplit) {
            hDiv = minHSplit;
        }
        else if (hDiv > ww - minTrtW) {
            hDiv = ww - minTrtW;
        }
    }
    else {
        hDiv = 0;
        if (showCat) {
            hDiv = config.hSplit;
            if ((typeOf(hDiv) != 'number') || (hDiv < minHSplit)) hDiv = uiLimits.defHSplit;
        }
    }

    if (manualV) {
        vDiv += sbh - 2;

        // Sanity check manual drag of divider
        if (vDiv > wh - minVSplit) {
            vDiv = wh - minVSplit;
        }
        else if (vDiv < tbh + minTrtH) {
            vDiv = tbh + minTrtH;
        }
    }
    else {
        vDiv = 0;
        if (showDet) {
            vDiv = config.vSplit;
            if ((typeOf(vDiv) != 'number') || (vDiv < minVSplit)) vDiv = uiLimits.defVSplit;
        }
        vDiv = wh - vDiv;
    }

    // Calculate torrent list size
    var trtw = ww - (hDiv + 2 + (showCat ? 5 : 0)) - (badIE ? 1 : 0),
        trth = vDiv - (tbh + sbh) - (!showDet ? 2 : 0) - (badIE ? 1 : 0);

    if (showCat) {
        $("mainCatList").show();

        if (trtw < minTrtW) {
            // Gracefully degrade if torrent list too small
            hDiv -= minTrtW - trtw;
            if (hDiv < minHSplit) {
                $("mainCatList").hide();
                showCat = false;
                trtw = ww - 2;
            }
            else {
                trtw = minTrtW;
            }
        }
    }

    if (showDet) {
        $("mainInfoPane").show();

        if (trth < minTrtH) {
            // Gracefully degrade if torrent list too small
            vDiv += minTrtH - trth;
            if (vDiv > wh - minVSplit) {
                $("mainInfoPane").hide();
                showDet = false;
                trth = wh - tbh - sbh - 2;
            }
            else {
                trth = minTrtH;
            }
        }
    }

    // Resize category/label list
    if (showCat) {
        if (hDiv) $("mainCatList").setStyle("width", hDiv - (badIE ? 2 : 0));

        if (tallCat) {
            $("mainCatList").setStyle("height", wh - tbh - sbh - 2);
        }
        else if (trth) {
            $("mainCatList").setStyle("height", trth);
        }
    }

    // Resize detailed info pane
    if (showDet) {
        var dw = ww - (showCat && tallCat ? hDiv + 5 : 0);
        if (vDiv) {
            var dh = wh - vDiv - $("mainInfoPane-tabs").getSize().y - (showSB ? 1 : 0) - 14;
            $("mainInfoPane-content").setStyles({"width": dw - 8, "height": dh});
            $("mainInfoPane-generalTab").setStyles({"width": dw - 10, "height": dh - 2});
            utWebUI.spdGraph.resizeTo(dw - 8, dh);
            $("mainInfoPane-loggerTab").setStyles({"width": dw - 14, "height": dh - 6});
            utWebUI.prsTable.resizeTo(dw - 10, dh - 2);
            utWebUI.flsTable.resizeTo(dw - 10, dh - 2);
        }
    }

    // Reposition dividers
    if ($("mainHDivider")) {
        $("mainHDivider").setStyles({
            "height": tallCat ? wh - tbh - sbh : trth + 2,
            "left": showCat ? hDiv + 2 : -10,
            "top": tbh
        });
    }

    if ($("mainVDivider")) {
        $("mainVDivider").setStyles({
            "width": tallCat && showCat ? ww - (hDiv + 5) : ww,
            "left": tallCat && showCat ? hDiv + 5 : 0,
            "top": showDet ? vDiv - sbh + 2 : -10
        });
    }

    // Store new divider position(s)
    if (hDiv && showCat && manualH) config.hSplit = hDiv;
    if (vDiv && showDet && manualV) config.vSplit = (wh - vDiv);

    // Resize torrent list
    utWebUI.trtTable.resizeTo(trtw, trth);
    if (!badIE) {
        // NOTE: We undefine the explicitly set width for modern browsers that have
        //       full page zoom, because if we specify an exact pixel width, the
        //       browser may not map the "virtual" pixels (the number of pixels the
        //       web application thinks it has due to zooming) to "physical" pixels
        //       in a manner such that the torrent jobs list would fit perfectly
        //       side by side with the category list.
        //
        //       An actual size is specified above in order to force the torrent
        //       list to resize when the horizontal divider is resized.

        utWebUI.trtTable.resizeTo(undefined, trth);
    }

    __resizeUI_ready__ = true;

}


//================================================================================
// USER INTERFACE SETUP
//================================================================================

var __executed_setupUserInterface__;

function setupCategoryUI() {
    //--------------------------------------------------
    // CATEGORY LIST
    //--------------------------------------------------

    ["mainCatList-categories", "mainCatList-labels"].each(function(k) {
        $(k).addEvent("mousedown", function(ev) {
            utWebUI.catListClick(ev, k);
        });
    });
}
function setupTorrentJobsUI() {
    //--------------------------------------------------
    // TORRENT JOBS LIST
    //--------------------------------------------------

    utWebUI.trtTable.create("mainTorList", utWebUI.trtColDefs, Object.append({
        "format": utWebUI.trtFormatRow.bind(utWebUI),
        "sortCustom": utWebUI.trtSortCustom.bind(utWebUI),
        "onColReset": utWebUI.trtColReset.bind(utWebUI),
        "onColResize": utWebUI.trtColResize.bind(utWebUI),
        "onColMove": utWebUI.trtColMove.bind(utWebUI),
        "onColToggle": utWebUI.trtColToggle.bind(utWebUI),
        "onKeyDown": function(ev) {
            switch (eventToKey(ev)) {
                case "alt enter":
                    utWebUI.showProperties();
                break;

                case "shift delete":
                case "delete":
                    utWebUI.removeDefault(ev.shift);
                break;
            }
        },
        "onSort": utWebUI.trtSort.bind(utWebUI),
        "onSelect": utWebUI.trtSelect.bind(utWebUI),
        "onDblClick": utWebUI.trtDblClk.bind(utWebUI)
    }, utWebUI.defConfig.torrentTable));

}
function setupDetailInfoPaneUI() {
    //--------------------------------------------------
    // DETAILED INFO PANE
    //--------------------------------------------------

    // -- Main Tabs

    utWebUI.mainTabs = new Tabs("mainInfoPane-tabs", {
        "tabs": {
              "mainInfoPane-generalTab" : ""
            , "mainInfoPane-peersTab"   : ""
            , "mainInfoPane-filesTab"   : ""
            , "mainInfoPane-speedTab"   : ""
            , "mainInfoPane-loggerTab"  : ""
        },
        "onChange": utWebUI.detPanelTabChange.bind(utWebUI)
    }).draw().show("mainInfoPane-generalTab");

    // -- General Tab

    $$("#mainInfoPane-generalTab td").addEvent("mousedown", function(ev) {
        if (!ev.isRightClick()) return;

        var targ = ev.target;
        if (targ.tagName.toLowerCase() !== "td")
            targ = targ.getParent("td");

        if (targ) {
            var span = targ.getElement("span");
            if (span) {
                ev.target = span;
                utWebUI.showGeneralMenu(ev);
            }
        }
    });

    // -- Peers Tab

    utWebUI.prsTable.create("mainInfoPane-peersTab", utWebUI.prsColDefs, Object.append({
        "format": utWebUI.prsFormatRow.bind(utWebUI),
        "onColReset": utWebUI.prsColReset.bind(utWebUI),
        "onColResize": utWebUI.prsColResize.bind(utWebUI),
        "onColMove": utWebUI.prsColMove.bind(utWebUI),
        "onColToggle": utWebUI.prsColToggle.bind(utWebUI),
        "onSort": utWebUI.prsSort.bind(utWebUI),
        "onSelect": utWebUI.prsSelect.bind(utWebUI)
    }, utWebUI.defConfig.peerTable));

    $("mainInfoPane-peersTab").addEvent("mousedown", function(ev) {
        if (ev.isRightClick() && ev.target.hasClass("stable-body")) {
            utWebUI.showPeerMenu(ev);
        }
    });

    // -- Files Tab

    utWebUI.flsTable.create("mainInfoPane-filesTab", utWebUI.flsColDefs, Object.append({
        "format": utWebUI.flsFormatRow.bind(utWebUI),
        "onColReset": utWebUI.flsColReset.bind(utWebUI),
        "onColResize": utWebUI.flsColResize.bind(utWebUI),
        "onColMove": utWebUI.flsColMove.bind(utWebUI),
        "onColToggle": utWebUI.flsColToggle.bind(utWebUI),
        "onSort": utWebUI.flsSort.bind(utWebUI),
        "onSelect": utWebUI.flsSelect.bind(utWebUI),
        "onDblClick": utWebUI.flsDblClk.bind(utWebUI)
    }, utWebUI.defConfig.fileTable));

    // -- Speed Tab
    if (utWebUI.defConfig.showSpeedGraph) {
        utWebUI.spdGraph.create("mainInfoPane-speedTab");
    }
    // -- Logger Tab

    Logger.init("mainInfoPane-loggerTab");
    $("mainInfoPane-loggerTab").addEvent("mousedown", function(ev) {
        ev.target.store("mousewhitelist", true);
    });

}
function setupDividers() {
    //--------------------------------------------------
    // DIVIDERS
    //--------------------------------------------------

    new Drag("mainHDivider", {
        "modifiers": {"x": "left", "y": ""},
        "onDrag": function() {
            resizeUI(this.value.now.x, null);
        },
        "onComplete": function() {
//          resizeUI(this.value.now.x, null);
            if (Browser.opera)
                utWebUI.saveConfig(true);
        }
    });

    new Drag("mainVDivider", {
        "modifiers": {"x": "", "y": "top"},
        "onDrag": function() {
            resizeUI(null, this.value.now.y);
        },
        "onComplete": function() {
//          resizeUI(null, this.value.now.y);
            if (Browser.opera)
                utWebUI.saveConfig(true);
        }
    });

}
function setupNonGuest() {
    //--------------------------------------------------
    // NON-GUEST SETUP
    //--------------------------------------------------

    __resizeUI_ready__ = true;

    if (isGuest) {
        resizeUI();
        return;
    }

}
function setupToolbar() {
    //--------------------------------------------------
    // TOOLBAR
    //--------------------------------------------------

    utWebUI.updateToolbar();

    // -- Buttons

    ["add", "addurl", "remove", "start", "pause", "stop", "queueup", "queuedown", "setting"].each(function(act) {
        $(act).addStopEvent("click", function(ev) {
            if (ev.target.hasClass("disabled")) {
                return;
            }

            var arg;
            switch (act) {
                case "add": utWebUI.showAddTorrent(); break;
                case "addurl": utWebUI.showAddURL(); break;
                case "setting": utWebUI.showSettings(); break;

                case "remove": utWebUI.removeDefault(ev.shift); break;

                case "queueup":
                case "queuedown":
                    arg = ev.shift;

                default:
                    utWebUI[act](arg);
            }
        });
    });

    // -- Toolbar Chevron

    $("tbchevron").addStopEvents({
        "mousedown": function(ev) {
            utWebUI.toolbarChevronShow(this);
        },
        "click": null
    });

    // -- Search Field

    $("query").addEvent("keydown", function(ev) {
        if (ev.key == "enter") {
            utWebUI.searchExecute();
        }
    });

    $("search").addStopEvents({
        "mousedown": function(ev) {
            if (ev.isRightClick()) {
                utWebUI.searchMenuShow(this);
            }
        },
        "click": function(ev) {
            utWebUI.searchExecute();
        }
    });

    $("searchsel").addStopEvents({
        "mousedown": function(ev) {
            utWebUI.searchMenuShow(this);
        },
        "click": null
    });
}
function setupDialogManager() {
    //--------------------------------------------------
    // DIALOG MANAGER
    //--------------------------------------------------

    DialogManager.init();

    ["About", "Add", "AddURL", "AddLabel", "Props", "Delete", "Settings"].each(function(k) {
        var isModal = ["Props"].contains(k);
        DialogManager.add(k, isModal, {
              "Add": function() { utWebUI.getDirectoryList(); }
            , "AddURL": function() { utWebUI.getDirectoryList(); }
            , "Settings": function() { utWebUI.stpanes.onChange(); }
        }[k]);
    });

}

function setupAddTorrentDialog() {
    //--------------------------------------------------
    // ADD TORRENT DIALOG
    //--------------------------------------------------

    // -- OK Button (File)

    $("ADD_FILE_OK").addEvent("click", function() {
        var dir = $("dlgAdd-basePath").value || 0;
        var sub = encodeURIComponent($("dlgAdd-subPath").get("value")); // TODO: Sanitize!

        console.log("encode torrent files");

        // should it hide the dialog now? YES
        DialogManager.hide("Add");
    });

    // -- Cancel Button (File)

    $("ADD_FILE_CANCEL").addEvent("click", function(ev) {
        DialogManager.hide("Add");
    });

    $("dlgAdd-form").addEvent("submit", Function.from(false));
}

function setupPropertiesDialog() {
    //--------------------------------------------------
    // PROPERTIES DIALOG
    //--------------------------------------------------

    // -- OK Button

    $("DLG_TORRENTPROP_01").addEvent("click", function() {
        DialogManager.hide("Props");
        utWebUI.setProperties();
    });

    // -- Cancel Button

    $("DLG_TORRENTPROP_02").addEvent("click", function(ev) {
        $("dlgProps").getElement(".dlg-close").fireEvent("click", ev);
            // Fire the "Close" button's click handler to make sure
            // controls are restored if necessary
    });

    // -- Close Button

    $("dlgProps").getElement(".dlg-close").addEvent("click", function(ev) {
        if (utWebUI.propID == "multi") {
            [11, 17, 18, 19].each(function(v) {
                $("DLG_TORRENTPROP_1_GEN_" + v).removeEvents("click");
            });
        }
        this.propID = "";
    });

    // -- Form Submission

    $("dlgProps-form").addEvent("submit", Function.from(false));

}

function setupDeleteTorrentDialog() {
    //--------------------------------------------------
    // DELETE TORRENT DIALOG
    //--------------------------------------------------
    
    // -- OK Button
    $("DELETE_TORRENT_OK").addEvent("click", function() {
        DialogManager.hide("Delete");
        
        var torrent = utweb.tables.torrent;
        var rows = utweb.tables.torrent.view.selectedRows();
        var value = $$('input[name=dlgDelete-torrent]:checked').get('value');
        
        torrent.remove_torrents(rows, { delete_data: value == "delete_data" });
    });

    // -- Cancel Button

    $("DELETE_TORRENT_CANCEL").addEvent("click", function(ev) {
        DialogManager.hide("Delete");
    });

    // -- Form Submission

    $("dlgDelete-form").addEvent("submit", Function.from(false));
}

function setupAddURLDialog() {
    //--------------------------------------------------
    // ADD URL DIALOG
    //--------------------------------------------------

    // -- OK Button (URL)
    $("ADD_URL_OK").addEvent("click", function() {
        if ($("dlgAddURL-url").get("value").trim().length > 0) {
            DialogManager.hide("AddURL");

            var param = {
                  "url": $("dlgAddURL-url").get("value")
                , "cookie": $("dlgAddURL-cookie").get("value")
                , "dir": $("dlgAddURL-basePath").value
                , "sub": $("dlgAddURL-subPath").get("value")
            };

            utWebUI.addURL(param, function() {
                $("dlgAddURL-url").set("value", "");
                $("dlgAddURL-cookie").set("value", "");
            });
        }
    });

    // -- Cancel Button (URL)

    $("ADD_URL_CANCEL").addEvent("click", function(ev) {
        DialogManager.hide("AddURL");
    });

    // -- Form Submission

    $("dlgAddURL-form").addEvent("submit", Function.from(false));

}

function setupSettings() {
    //--------------------------------------------------
    // SETTINGS DIALOG
    //--------------------------------------------------

    // -- OK Button

    $("DLG_SETTINGS_03").addEvent("click", function() {
        //DialogManager.hide("Settings");
        utWebUI.hideSettings();
        utWebUI.setSettings();
    });
    
    // -- Save Button

    // -- Cancel Button

    $("DLG_SETTINGS_04").addEvent("click", function(ev) {
        utWebUI.hideSettings(true);
        $("dlgSettings").getElement(".dlg-close").fireEvent("click", ev);
            // Fire the "Close" button's click handler to make sure
            // controls are restored if necessary
    });

    // -- Apply Button

    $("DLG_SETTINGS_05").addEvent("click", function(ev) {
        utWebUI.setSettings();
    });

    // -- Close Button

    $("dlgSettings").getElement(".dlg-close").addEvent("click", function(ev) {
        utWebUI.hideSettings(true);
        //utWebUI.loadSettings();
    });

    // -- Form Submission

    $("dlgSettings-form").addEvent("submit", Function.from(false));

    // -- Pane Selector

    utWebUI.stpanes = new Tabs("dlgSettings-menu", {
        "tabs": {
              "dlgSettings-General"     : ""
            , "dlgSettings-Connection"  : "Connection"
            , "dlgSettings-Directories" : "Directories"
            , "dlgSettings-Advanced"    : ""
            , "dlgSettings-WebUI"       : "" 
            , "dlgSettings-Plugins"     : "Plugins"
        },
        "lazyshow": false,
        "onChange": utWebUI.settingsPaneChange.bind(utWebUI)
    }).draw().show("dlgSettings-General");

    // -- General

    var langArr = [];
    $each(LANG_LIST, function(lang, code) {
        langArr.push({lang: lang, code: code});
    });
    langArr.sort(function(x, y) {
        return (x.lang < y.lang ? -1 : (x.lang > y.lang ? 1 : 0));
    });

    var langSelect = $("webui.lang");
    langSelect.options.length = langArr.length;
    Array.each(langArr, function(v, k) {
        langSelect.options[k] = new Option(v.lang, v.code, false, false);
    });
    langSelect.set("value", utWebUI.defConfig.lang);

    // -- Plugin table
    utWebUI.plgTable.create("dlgSettings-pluginList", utWebUI.plgTableColDefs, Object.append({
        "format": utWebUI.plgFormatRow.bind(utWebUI),
        "onColReset": utWebUI.plgColReset.bind(utWebUI),
        "onSelect": utWebUI.plgSelect.bind(utWebUI)
    }, utWebUI.defConfig.plgTable));

    var plgSize = $("dlgSettings-Plugins").getDimensions({computeSize: true});
    utWebUI.plgTable.resizeTo(plgSize.x - 15, plgSize.y - 70);
    
    // -- Advanced Options

    utWebUI.advOptTable.create("dlgSettings-advOptList", utWebUI.advOptColDefs, Object.append({
        "format": utWebUI.advOptFormatRow.bind(utWebUI),
        "onColReset": utWebUI.advOptColReset.bind(utWebUI),
        "onSelect": utWebUI.advOptSelect.bind(utWebUI),
        "onDblClick": utWebUI.advOptDblClk.bind(utWebUI)
    }, utWebUI.defConfig.advOptTable));

    $("DLG_SETTINGS_A_ADVANCED_05").addEvent("click", utWebUI.advOptChanged.bind(utWebUI));
    $("dlgSettings-advTrue").addEvent("click", utWebUI.advOptChanged.bind(utWebUI));
    $("dlgSettings-advFalse").addEvent("click", utWebUI.advOptChanged.bind(utWebUI));

    var advSize = $("dlgSettings-Advanced").getDimensions({computeSize: true});
    utWebUI.advOptTable.resizeTo(advSize.x - 15, advSize.y - 70);

    // -- Linked Controls

    var linkedEvent = Browser.ie ? "click" : "change";

    // -- Miscellaneous

    _unhideSetting([
        // General
        "webui.lang"

        // BitTorrent
        , "enable_bw_management"

        // Transfer Cap - Usage history
        , "multi_day_transfer_mode"
        , "total_uploaded_history"
        , "total_downloaded_history"
        , "total_updown_history"
        , "history_period"
        , "DLG_SETTINGS_7_TRANSFERCAP_12"

        // Advanced
        , "DLG_SETTINGS_A_ADVANCED_02"

        // Web UI
        , "webui.showToolbar"
        , "webui.showCategories"
        , "webui.showDetails"
        , "webui.showStatusBar"
        , "webui.maxRows"
        , "webui.updateInterval"
        , "webui.useSysFont"
    ]);

}

function setupStatusBar() {

    //--------------------------------------------------
    // STATUS BAR
    //--------------------------------------------------

    $("mainStatusBar-menu").addStopEvent("mousedown", function(ev) {
        return utWebUI.statusMenuShow(ev);
    });

    $("mainStatusBar-download").addStopEvent("mousedown", function(ev) {
        return utWebUI.statusDownloadMenuShow(ev);
    });

    $("mainStatusBar-upload").addStopEvent("mousedown", function(ev) {
        return utWebUI.statusUploadMenuShow(ev);
    });

}

function setupUserInterface() {

    if (__executed_setupUserInterface__) return;
    __executed_setupUserInterface__ = true;

    document.title = g_winTitle;
    setupCategoryUI();
    setupTorrentJobsUI();
    setupDetailInfoPaneUI();
    setupDividers();
    setupNonGuest();
    setupToolbar();
    setupDialogManager();
    setupAddTorrentDialog();
    setupPropertiesDialog();
    setupDeleteTorrentDialog();
    setupAddURLDialog();
    
    setupSettings();
    setupStatusBar();

    resizeUI();

}

function _link(obj, defstate, list, ignoreLabels, reverse) {
    ignoreLabels = ignoreLabels || [];
    var disabled = true, tag = obj.get("tag");
    if (tag == "input") {
        if (obj.type == "checkbox" || obj.type == "radio")
            disabled = !obj.checked || obj.disabled;
            if (reverse)
                disabled = !disabled;
    } else if (tag == "select") {
        disabled = (obj.get("value") == defstate);
    } else {
        return;
    }
    var element;
    for (var i = 0, j = list.length; i < j; i++) {
        if (!(element = $(list[i]))) continue;
        if (element.type != "checkbox")
            element[(disabled ? "add" : "remove") + "Class"]("disabled");
        element.disabled = disabled;
        element.fireEvent(((tag == "input") && Browser.ie) ? "click" : "change");
        if (ignoreLabels.contains(list[i])) continue;
        var label = element.getPrevious();
        if (!label || (label.get("tag") != "label")) {
            label = element.getNext();
            if (!label || (label.get("tag") != "label")) continue;
        }
        label[(disabled ? "add" : "remove") + "Class"]("disabled");
    }
}

function _unhideSetting(obj) {
    Array.from(obj).each(function(ele) {
        ele = $(ele);
        if (!ele) return;

        ele = ele.getParent();
        while (ele && !ele.hasClass("settings-pane") && ele.getStyle("display") === "none") {
            ele.show();
            ele = ele.getParent();
        }

        if (ele.hasClass("settings-pane"))
            ele.fireEvent("show");
    }, this);
}


//================================================================================
// LANGUAGE STRING LOADING
//================================================================================

function loadTorrentLangStrings() {
    //--------------------------------------------------
    // TORRENT JOBS LIST
    //--------------------------------------------------

    if (utWebUI.trtTable.tb.body) { utWebUI.trtTable.refreshRows(); }
    utWebUI.trtTable.setConfig({
        "resetText": L_("MENU_RESET"),
        "colText": {
              "name"         : L_("OV_COL_NAME")
            , "order"        : L_("OV_COL_ORDER")
            , "size"         : L_("OV_COL_SIZE")
            , "remaining"    : L_("OV_COL_REMAINING")
            , "done"         : L_("OV_COL_DONE")
            , "status"       : L_("OV_COL_STATUS")
            , "seeds"        : L_("OV_COL_SEEDS")
            , "peers"        : L_("OV_COL_PEERS")
            , "seeds_peers"  : L_("OV_COL_SEEDS_PEERS")
            , "downspeed"    : L_("OV_COL_DOWNSPD")
            , "upspeed"      : L_("OV_COL_UPSPD")
            , "eta"          : L_("OV_COL_ETA")
            , "downloaded"   : L_("OV_COL_DOWNLOADED")
            , "uploaded"     : L_("OV_COL_UPPED")
            , "ratio"        : L_("OV_COL_SHARED")
            , "availability" : L_("OV_COL_AVAIL").split("||")[1]
            , "label"        : L_("OV_COL_LABEL")
            , "added"        : L_("OV_COL_DATE_ADDED")
            , "completed"    : L_("OV_COL_DATE_COMPLETED")
            , "url"          : L_("OV_COL_SOURCE_URL")
        }
    });

}
function loadCategoryStrings() {
    //--------------------------------------------------
    // CATEGORY LIST
    //--------------------------------------------------

    _loadStrings("text", [
          "OV_CAT_ALL"
        , "OV_CAT_DL"
        , "OV_CAT_COMPL"
        , "OV_CAT_ACTIVE"
        , "OV_CAT_INACTIVE"
        , "OV_CAT_NOLABEL"
    ]);
}
function loadDetailPaneStrings() {
    //--------------------------------------------------
    // DETAILED INFO PANE
    //--------------------------------------------------

    // -- Tab Titles

    
    if (utWebUI.mainTabs) { 
        var maintstr = L_("OV_TABS").split("||");
        utWebUI.mainTabs.setNames({
              "mainInfoPane-generalTab" : maintstr[0]
            , "mainInfoPane-peersTab"   : maintstr[2]
            , "mainInfoPane-filesTab"   : maintstr[4]
            , "mainInfoPane-speedTab"   : maintstr[5]
            , "mainInfoPane-loggerTab"  : maintstr[6]
        });
    }

    // -- General Tab

    _loadStrings("text", [
          "GN_TRANSFER"
        , "GN_TP_01"
        , "GN_TP_02"
        , "GN_TP_03"
        , "GN_TP_04"
        , "GN_TP_05"
        , "GN_TP_06"
        , "GN_TP_07"
        , "GN_TP_08"

        , "GN_GENERAL"
        , "GN_TP_09"
        , "GN_TP_10"
    ]);

    // -- Peers Tab

    if (utWebUI.prsTable.tb.body) { 
utWebUI.prsTable.refreshRows(); 
    utWebUI.prsTable.setConfig({
        "resetText": L_("MENU_RESET"),
        "colText": {
              "ip"         : L_("PRS_COL_IP")
            , "port"       : L_("PRS_COL_PORT")
            , "client"     : L_("PRS_COL_CLIENT")
            , "flags"      : L_("PRS_COL_FLAGS")
            , "pcnt"       : L_("PRS_COL_PCNT")
            , "relevance"  : L_("PRS_COL_RELEVANCE")
            , "downspeed"  : L_("PRS_COL_DOWNSPEED")
            , "upspeed"    : L_("PRS_COL_UPSPEED")
            , "reqs"       : L_("PRS_COL_REQS")
            , "waited"     : L_("PRS_COL_WAITED")
            , "uploaded"   : L_("PRS_COL_UPLOADED")
            , "downloaded" : L_("PRS_COL_DOWNLOADED")
            , "hasherr"    : L_("PRS_COL_HASHERR")
            , "peerdl"     : L_("PRS_COL_PEERDL")
            , "maxup"      : L_("PRS_COL_MAXUP")
            , "maxdown"    : L_("PRS_COL_MAXDOWN")
            , "queued"     : L_("PRS_COL_QUEUED")
            , "inactive"   : L_("PRS_COL_INACTIVE")
        }
    });
}

    // -- Files Tab

    if (utWebUI.flsTable.tb.body) { utWebUI.flsTable.refreshRows();
    utWebUI.flsTable.setConfig({
        "resetText": L_("MENU_RESET"),
        "colText": {
              "name"    : L_("FI_COL_NAME")
            , "size"    : L_("FI_COL_SIZE")
            , "done"    : L_("FI_COL_DONE")
            , "pcnt"    : L_("FI_COL_PCNT")
            , "firstpc" : L_("FI_COL_FIRSTPC")
            , "numpcs"  : L_("FI_COL_NUMPCS")
            , "prio"    : L_("FI_COL_PRIO")
        }
    });
}

    // -- Speed Tab
if (utWebUI.spdGraph) {
    utWebUI.spdGraph.setLabels(
          L_("OV_COL_UPSPD")
        , L_("OV_COL_DOWNSPD")
    );
}

}

function loadAboutStrings() {
    //--------------------------------------------------
    // ABOUT DIALOG
    //--------------------------------------------------

    _loadStrings("text", [
          "DLG_ABOUT_VERSION_LEGEND"
        , "DLG_ABOUT_VERSION_VERSION"
        , "DLG_ABOUT_VERSION_REVISION"
        , "DLG_ABOUT_VERSION_BUILD_DATE"
        , "DLG_ABOUT_VERSION_PEER_ID"
        , "DLG_ABOUT_VERSION_USER_AGENT"
        , "DLG_ABOUT_UPNP_EXTERNAL_ADDRESS"
        , "DLG_ABOUT_UI_REVISION"
    ]);
    console.warn('RUNNING!!YAY!');
    _loadStrings("text", { "show_about": "about" });
}

function loadMiscStrings() {

    
    //--------------------------------------------------
    // STATUS
    //--------------------------------------------------

    utWebUI.updateStatusBar();

    //--------------------------------------------------
    // NON-GUEST SETUP
    //--------------------------------------------------

    if (isGuest) return;

    //--------------------------------------------------
    // TOOLBAR
    //--------------------------------------------------

    _loadStrings("title", {
          "add"           : "OV_TB_ADDTORR"
        , "addurl"        : "OV_TB_ADDURL"
        , "remove"        : "OV_TB_REMOVE"
        , "start"         : "OV_TB_START"
        , "pause"         : "OV_TB_PAUSE"
        , "stop"          : "OV_TB_STOP"
        , "queueup"       : "OV_TB_QUEUEUP"
        , "queuedown"     : "OV_TB_QUEUEDOWN"
        , "setting"       : "OV_TB_PREF"
    });
}

function loadDialogStrings() {
    //--------------------------------------------------
    // ALL DIALOGS
    //--------------------------------------------------

    // -- Titles

    _loadStrings("text", {
          "dlgAdd-head"           : "OV_TB_ADDTORR"
        , "dlgAddURL-head"        : "OV_TB_ADDURL"
        , "dlgProps-head"         : "DLG_TORRENTPROP_00"
        , "dlgSettings-head"      : "OV_TB_PREF"
    });

    // -- [ OK | Cancel | Apply | Close ] Buttons

    _loadStrings("value", {
        // Add
          "ADD_FILE_OK"     : "DLG_BTN_OK"
        , "ADD_FILE_CANCEL" : "DLG_BTN_CANCEL"
        
        // Add LABEL
        , "ADD_LABEL_OK"      : "DLG_BTN_OK"
        , "ADD_LABEL_CANCEL"  : "DLG_BTN_CANCEL"

        // Add URL
        , "ADD_URL_OK"      : "DLG_BTN_OK"
        , "ADD_URL_CANCEL"  : "DLG_BTN_CANCEL"

        // Properties
        , "DLG_TORRENTPROP_01" : "DLG_BTN_OK"
        , "DLG_TORRENTPROP_02" : "DLG_BTN_CANCEL"

        // Settings
        , "DLG_SETTINGS_03" : "DLG_BTN_OK"
        , "DLG_SETTINGS_04" : "DLG_BTN_CANCEL"
        , "DLG_SETTINGS_05" : "DLG_BTN_APPLY"
    });

    //--------------------------------------------------
    // ABOUT DIALOG
    //--------------------------------------------------

    $("dlgAbout-version").set("text", "v" + CONST.VERSION + (CONST.BUILD ? " (" + CONST.BUILD + ")" : ""));

    //--------------------------------------------------
    // PROPERTIES DIALOG
    //--------------------------------------------------

    _loadStrings("text", [
          "DLG_TORRENTPROP_1_GEN_01"
        , "DLG_TORRENTPROP_1_GEN_03"
        , "DLG_TORRENTPROP_1_GEN_04"
        , "DLG_TORRENTPROP_1_GEN_06"
        , "DLG_TORRENTPROP_1_GEN_08"
        , "DLG_TORRENTPROP_1_GEN_10"
        , "DLG_TORRENTPROP_1_GEN_11"
        , "DLG_TORRENTPROP_1_GEN_12"
        , "DLG_TORRENTPROP_1_GEN_14"
        , "DLG_TORRENTPROP_1_GEN_16"
        , "DLG_TORRENTPROP_1_GEN_17"
        , "DLG_TORRENTPROP_1_GEN_18"
        , "DLG_TORRENTPROP_1_GEN_19"
    ]);
}

function loadSettingStrings() {
    //--------------------------------------------------
    // SETTINGS DIALOG
    //--------------------------------------------------

    utWebUI.stpanes.setNames({
          "dlgSettings-General"     : L_("ST_CAPT_GENERAL")
        //, "dlgSettings-UISettings"  : L_("ST_CAPT_UI_SETTINGS")
        //, "dlgSettings-Directories" : L_("ST_CAPT_FOLDER")
        //, "dlgSettings-Connection"  : L_("ST_CAPT_CONNECTION")
        //, "dlgSettings-Bandwidth"   : L_("ST_CAPT_BANDWIDTH")
        //, "dlgSettings-BitTorrent"  : L_("ST_CAPT_BITTORRENT")
        //, "dlgSettings-TransferCap" : L_("ST_CAPT_TRANSFER_CAP")
        //, "dlgSettings-Queueing"    : L_("ST_CAPT_QUEUEING")
        , "dlgSettings-WebUI"       : L_("ST_CAPT_WEBUI")
        //, "dlgSettings-Scheduler"   : L_("ST_CAPT_SCHEDULER")
        , "dlgSettings-Advanced"    : L_("ST_CAPT_ADVANCED")
        //, "dlgSettings-UIExtras"    : "&nbsp;&nbsp;&nbsp;&nbsp;" + L_("ST_CAPT_UI_EXTRAS") // TODO: Use CSS to indent instead of modifying the string directly...
        //, "dlgSettings-DiskCache"   : "&nbsp;&nbsp;&nbsp;&nbsp;" + L_("ST_CAPT_DISK_CACHE") // TODO: Use CSS to indent instead of modifying the string directly...
        //, "dlgSettings-RunProgram"  : "&nbsp;&nbsp;&nbsp;&nbsp;" + L_("ST_CAPT_RUN_PROGRAM") // TODO: Use CSS to indent instead of modifying the string directly...
    });

    _loadStrings("text", [
        // General
          "DLG_SETTINGS_1_GENERAL_01"
        , "DLG_SETTINGS_1_GENERAL_02"
        , "DLG_SETTINGS_1_GENERAL_10"
        , "DLG_SETTINGS_1_GENERAL_11"
        , "DLG_SETTINGS_1_GENERAL_12"
        , "DLG_SETTINGS_1_GENERAL_13"
        , "DLG_SETTINGS_1_GENERAL_17"
        , "DLG_SETTINGS_1_GENERAL_18"
        , "DLG_SETTINGS_1_GENERAL_19"
        , "DLG_SETTINGS_1_GENERAL_20"

        // Web UI
        , "DLG_SETTINGS_9_WEBUI_01"
        , "DLG_SETTINGS_9_WEBUI_02"
        , "DLG_SETTINGS_9_WEBUI_03"
        , "DLG_SETTINGS_9_WEBUI_05"
        , "DLG_SETTINGS_9_WEBUI_07"
        , "DLG_SETTINGS_9_WEBUI_09"
        , "DLG_SETTINGS_9_WEBUI_10"
        , "DLG_SETTINGS_9_WEBUI_12"

        , "MM_OPTIONS_SHOW_CATEGORY"
        , "MM_OPTIONS_SHOW_DETAIL"
        , "MM_OPTIONS_SHOW_STATUS"
        , "MM_OPTIONS_SHOW_TOOLBAR"

        // Advanced
        , "DLG_SETTINGS_A_ADVANCED_01"
        , "DLG_SETTINGS_A_ADVANCED_02"
        , "DLG_SETTINGS_A_ADVANCED_03"
        , "DLG_SETTINGS_A_ADVANCED_04"
    ]);

    // -- Advanced Options

    if (utWebUI.advOptTable.tb.body) { utWebUI.advOptTable.refreshRows(); }
    utWebUI.advOptTable.setConfig({
        "resetText": L_("MENU_RESET"),
        "colText": {
              "name"  : L_("ST_COL_NAME")
            , "value" : L_("ST_COL_VALUE")
        }
    });

    // -- Buttons
    _loadStrings("value", [
          "DLG_SETTINGS_SAVE" // "Save settings"
        , "DLG_SETTINGS_A_ADVANCED_05" // "Set"
    ]);
    
    _loadStrings("text", [
          "DLG_SETTINGS_MENU_TITLE" // "Settings"
    ]);
}

function loadGlobalStrings() {
    g_perSec = "/" + L_("TIME_SECS").replace(/%d/, "").trim();
    g_dayCodes = L_("ST_SCH_DAYCODES").split("||");
    g_dayNames = L_("ST_SCH_DAYNAMES").split("||");
    g_schLgndEx = {
          "full"    : L_("ST_SCH_LGND_FULLEX")
        , "limited" : L_("ST_SCH_LGND_LIMITEDEX")
        , "off"     : L_("ST_SCH_LGND_OFFEX")
        , "seeding" : L_("ST_SCH_LGND_SEEDINGEX")
    };
}

function loadLangStrings(reload, sTableLoad, newLang) {
    if (reload) {
        var loaded = false;
        var lang_path = 'lang/';
        Asset.javascript(lang_path + reload.lang + ".js", {
            "onload": function() {
                if (loaded) return;
                loaded = true;
                var newLang = reload.lang;
                loadLangStrings(null, ! window.utweb, newLang);
                if (reload.onload) reload.onload();
            }
        });
        return;
    }
    loadGlobalStrings();
    loadCategoryStrings();
    if (sTableLoad) {
        loadTorrentLangStrings();
        loadDetailPaneStrings();
    }
    loadMiscStrings();
    loadDialogStrings();
    loadSettingStrings();
    if (window.utweb) {
        utweb.change_language(newLang);
    }

}

function _loadComboboxStrings(id, vals, def) {
    try {
        var ele = $(id);

        ele.options.length = 0;
        $each(vals, function(v, k) {
            if (!v) return;
            switch (typeOf(v)) {
                case "array":
                    ele.options[ele.options.length] = new Option(v[1], v[0], false, false);
                break;

                default:
                    ele.options[ele.options.length] = new Option(v, k, false, false);
            }
        });

        ele.set("value", def || 0);
    }
    catch(e) {
        console.log("Error attempting to assign values to combobox with id='" + id + "'... ");
        console.log(e.name + ": " + e.message);
    }
}

function _loadStrings(prop, strings) {
    var fnload;
    switch (typeOf(strings)) {
        case 'object':
            fnload = function(val, key) {
                $(key).set(prop, L_(val));
            };
        break;

        default:
            strings = Array.from(strings);
            fnload = function(val) {
                $(val).set(prop, L_(val));
            };
    }

    $each(strings, function(val, key) {
        try {
            fnload(val, key);
        }
        catch(e) {
            console.log("Error attempting to assign string '" + val + "' to element...");
        }
    });
}
