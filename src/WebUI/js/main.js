Object.append(Element.NativeEvents, {
    dragenter: 2,
    dragleave: 2,
    drop: 2,
    hashchange: 2
});

var Pages = {};

window.addEvent('hashchange', function() {
    var hash = location.hash;

    if(hash == "")
        return;

    navigate(hash);
});

window.addEvent('domready', function() {
    $('modal-back').setStyle('display', 'none');

    var roar = new Roar({
        position: 'bottomRight'
    });

    var tm = new TorrentManager();
    var net = new Network();

    Pages["/dashboard.html"] = new Dashboard();
    Pages["/settings.html"] = new Settings();

    // navigation defaults
    var hash = location.hash;
    if(hash == "") hash = "#!/dashboard.html";
    navigate(hash);

    $$('.page-navigator').addEvent('click', function(e) {
        e.preventDefault();
        e.stopPropagation();

        var page = this.get("href");
        location.hash = "#!" + page;
    });

    $('addTorrents').addEvent('click', function(){
        var dialog = new AddTorrentsDialog(roar, net, {
            loaded: function() {
                dialog.showDialog();
            }
        });
    });

    
});

function navigate(hash){
    hash = hash.substring(1);

    if(hash.substring(0, 1) == "!"){
        var page = hash.substring(1);
        setPage(page);
    }
}

function setPage(page) {
    if(Pages[page] === undefined) return;

    var obj = Pages[page];
    obj.load();
}