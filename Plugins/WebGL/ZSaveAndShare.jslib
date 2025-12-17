mergeInto(LibraryManager.library, {

   zappar_sns_initialize: function(canvas, unityObject, onSavedFunc, onSharedFunc, onClosedFunc) {
       var canva = document.querySelector(UTF8ToString(canvas));
       if(typeof canva === 'undefined' || canva === null) {
           window.snsInitialized=false;
           console.log(UTF8ToString(canvas)+" not found in document");
           return false;
       }
       window.unityCanvas = canva;
       window.unitySNSObjectListener = UTF8ToString(unityObject);
       window.unitySNSOnSavedFunc = UTF8ToString(onSavedFunc);
       window.unitySNSOnSharedFunc = UTF8ToString(onSharedFunc);
       window.unitySNSOnClosedFunc = UTF8ToString(onClosedFunc);
       if (typeof ZapparSharing === 'undefined' || ZapparSharing === null) {
            var scr = document.createElement("script");
            scr.src="https://libs.zappar.com/zappar-sharing/1.1.8/zappar-sharing.min.js";
            scr.addEventListener('load', function() {
                console.log("Zappar-sharing version 1.1.8 added");
                window.snsInitialized = true;
            });
            document.body.appendChild(scr);
        }else{
            window.snsInitialized = true;
        }
        return true;
   },

   zappar_sns_is_initialized : function() {
       if(typeof window.snsInitialized === 'undefined') return false;
       return window.snsInitialized;
   },

   zappar_sns_jpg_snapshot : function(img, size, quality) {
       if(typeof window.snsInitialized === 'undefined' || window.snsInitialized === false) return;
        //window.snapUrl = window.unityCanvas.toDataURL('image/jpeg',quality);
        var binary = '';
        for (var i = 0; i < size; i++)
            binary += String.fromCharCode(HEAPU8[img + i]);
        window.snapUrl = 'data:image/jpeg;base64,' + btoa(binary);
   },

   zappar_sns_png_snapshot : function(img, size, quality) {
       if(typeof window.snsInitialized === 'undefined' || window.snsInitialized === false) return;
        //window.snapUrl = window.unityCanvas.toDataURL('image/png',quality);
        var binary = '';
        for (var i = 0; i < size; i++)
            binary += String.fromCharCode(HEAPU8[img + i]);
        window.snapUrl = 'data:image/png;base64,' + btoa(binary);
   },

   zappar_sns_open_snap_prompt : function() {
       if(typeof window.snsInitialized === 'undefined' || window.snsInitialized === false) return;
       ZapparSharing({
        data: window.snapUrl,
        onSave: () => {
            window.uarGameInstance.SendMessage(window.unitySNSObjectListener, window.unitySNSOnSavedFunc);
        },
        onShare: () => {
          window.uarGameInstance.SendMessage(window.unitySNSObjectListener, window.unitySNSOnSharedFunc);
        },
        onClose: () => {
          window.uarGameInstance.SendMessage(window.unitySNSObjectListener, window.unitySNSOnClosedFunc);
        },
       });
   },

   zappar_sns_open_video_prompt : function() {
       if(typeof window.snsInitialized === 'undefined' || window.snsInitialized === false) return;
       ZapparSharing({
        data: window.videoRecUrl,
        onSave: () => {
            window.uarGameInstance.SendMessage(window.unitySNSObjectListener, window.unitySNSOnSavedFunc);
        },
        onShare: () => {
          window.uarGameInstance.SendMessage(window.unitySNSObjectListener, window.unitySNSOnSharedFunc);
        },
        onClose: () => {
          window.uarGameInstance.SendMessage(window.unitySNSObjectListener, window.unitySNSOnClosedFunc);
        },
       });
   },

   upload_external_screenshot: function(imgPtr, size) {
       if(typeof window.snsInitialized === 'undefined') {
           window.snsInitialized = true; // Forzamos inicialización si no se hizo
       }
       
       var binary = '';
       for (var i = 0; i < size; i++) {
           binary += String.fromCharCode(HEAPU8[imgPtr + i]);
       }
       
       window.snapUrl = 'data:image/png;base64,' + btoa(binary);
       
       console.log("Imagen externa cargada exitosamente en window.snapUrl");
   },

zappar_sns_share_native: function(text) {
        if (!window.snapUrl) {
            console.error("No hay captura de pantalla");
            return;
        }

        var shareText = UTF8ToString(text);

        // Convertir Base64 a Blob de forma manual para asegurar compatibilidad
        var parts = window.snapUrl.split(';base64,');
        var contentType = parts[0].split(':')[1];
        var raw = window.atob(parts[1]);
        var rawLength = raw.length;
        var uInt8Array = new Uint8Array(rawLength);

        for (var i = 0; i < rawLength; ++i) {
            uInt8Array[i] = raw.charCodeAt(i);
        }

        var blob = new Blob([uInt8Array], { type: contentType });
        // USAR UN NOMBRE DINÁMICO Y CLARO
        var file = new File([blob], "Screenshot_Game.png", { type: "image/png" });

        if (navigator.canShare && navigator.canShare({ files: [file] })) {
            navigator.share({
                files: [file],
                title: 'Check my score!',
                text: shareText
            })
            .then(function() {
                var unityInstance = window.uarGameInstance || window.unityInstance || window.gameInstance;
                if (unityInstance) unityInstance.SendMessage(window.unitySNSObjectListener, window.unitySNSOnSharedFunc);
            })
            .catch(function(error) { 
                console.log('Share failed:', error);
                // Si falla el menú, descargamos como plan B
                var a = document.createElement('a');
                a.href = window.snapUrl;
                a.download = "Screenshot_Game.png";
                a.click();
            });
        } else {
            // FALLBACK PARA DESKTOP / TWITTER
            var twitterUrl = "https://twitter.com/intent/tweet?text=" + encodeURIComponent(shareText);
            window.open(twitterUrl, '_blank');
            
            // Descargar la imagen automáticamente ya que no se puede "adjuntar" a Twitter vía URL
            var a = document.createElement('a');
            a.href = window.snapUrl;
            a.download = "Screenshot_Game.png";
            a.click();
            alert("Twitter no permite adjuntar imágenes automáticamente. Se ha descargado tu captura para que la subas.");
        }
    },
});