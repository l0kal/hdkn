Object.append(Element.NativeEvents, {
    dragenter: 2,
    dragleave: 2,
    drop: 2
});

var Pages = {};

window.addEvent('domready', function() {
    $('modal-back').setStyle('display', 'none');

    var roar = new Roar({
        position: 'bottomRight'
    });

    var tm = new TorrentManager();
    var net = new Network();

    Pages["dash"] = new Dashboard();
    Pages["settings"] = new Settings();

    setPage(Pages["dash"]);

    $('page-navigator').addEvent('click', function() {
        alert(this);
    });

    $('addTorrents').addEvent('click', function(){
        var dialog = new AddTorrentsDialog(roar, net, {
            loaded: function() {
                dialog.showDialog();
            }
        });
    });

    roar.alert("Hadouken", "All done sir.");
});

function setPage(page) {
    page.load();
}