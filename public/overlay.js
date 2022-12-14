var genvidClient;

fetch("/api/public/channels/join", { method: "post" })
    .then(function (data) { return data.json() })
    .then(function (response) {
        genvidClient = genvid.createGenvidClient(response.info, response.uri, response.token, "video_player");
        genvidClient.start();
    })
    .catch(function (e) { console.log(e) })

var buttonElements = document.querySelectorAll("[type=setbutton]");
buttonElements.forEach((buttonElement, index) => {

    let top = ((Math.floor(index / 5)) * 6) + 3;
    let left = ((index % 5) * 6) + 3;

    buttonElement.setAttribute('style','top:'+top+'vw;left:'+left+'vw;');
    buttonElement.onclick = function () {

        if (buttonElement.innerText == "None")
        {
            buttonElement.innerText = 'Pillar';
            buttonElement.setAttribute('style', 'background-color:orange;top:'+top+'vw;left:'+left+'vw;');
        }else{
            buttonElement.innerText = "None";
            buttonElement.setAttribute('style', 'background-color:white;top:'+top+'vw;left:'+left+'vw;');
        }
    }
});

var sendButtonElement = document.getElementById("send");
sendButtonElement.onclick = function () {
    var arr = Array.prototype.map.call(buttonElements, (b) => b.innerText == "None" ? 0 : 1);

    var levelDataArrayStr = arr.join(',');

    genvidClient.sendEvent([{
        "key": ["levelData"],
        "value": levelDataArrayStr
    }])
};



