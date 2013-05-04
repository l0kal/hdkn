var AddTorrentsDialog = new Class({
    Extends: Dialog,

    initialize: function(roar, network, options) {
        this.roar = roar;
        this.network = network;
        this.torrents = {};

        this.parent("/dialogs/addtorrentsdialog.html", options);
    },

    load: function() {
        var cancel = this.element.getElementById('adt_cancel_btn');
        var submit = this.element.getElementById('adt_submit_btn');
        var dropArea = this.element.getElementById('adt_droparea');

        cancel.addEvent('click', function() {
            this.hide();
        }.bind(this));

        submit.addEvent('click', function() {
            submit.set('disabled', 'disabled');
            this.submit();
        }.bind(this));

        dropArea.addEventListener('drop', function(e) {
            var that = this;

            e.stopPropagation();
            e.preventDefault();

            for(var i = 0; i < e.dataTransfer.files.length; i++) {
                that.addFile(e.dataTransfer.files[i]);
            }
        }.bind(this));
    },

    addFile: function(file) {
        var reader = new FileReader();
        reader.onload = (function(f) {
            return function(e) {

                var name = file.name;
                var data = e.target.result.split(',')[1];

                if(this.torrents[name] !== undefined){
                    return;
                }

                this.torrents[name] = {
                    name: name,
                    data: data
                };

                var el = new Element('li', {
                    html: escape(file.name)
                });

                el.inject(this.element.getElementById('adt_files'));

            }.bind(this);
        }.bind(this))(file);

        reader.readAsDataURL(file);
    },

    submit: function(){
        var kc = Object.getLength(this.torrents);
        var cc = 0;

        Object.each(this.torrents, function(v, k) {
            this.network.post("/api/torrents", { data: v.data }, function(result) {
                this.roar.alert("Torrent added", k);

                cc+=1;
                if(cc >= kc) {
                    this.hide();
                }
            }.bind(this));
        }.bind(this));
    }
})