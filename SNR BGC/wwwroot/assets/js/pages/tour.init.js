$(document).ready(function() {
    hopscotch.startTour({
        id: "my-intro",
        steps: [{
            target: "logo-tour",
            title: "Logo Here",
            content: "You can find here status of user who's currently online.",
            placement: "bottom",
            yOffset: 10,
            xOffset: -105,
            arrowOffset: "center"
        }, {
            target: "display-title-tour",
            title: "Display Text",
            content: "Click on the button and make sidebar navigation small.",
            placement: "top",
            zindex: 9999
        }, {
            target: "thankyou-tour",
            title: "Thank you !",
            content: "Here you can change theme skins and other features.",
            placement: "top",
            zindex: 999,
            yOffset: -10,
            xOffset: -105,
            arrowOffset: "center"
        }],
        showPrevButton: !0
    })
});