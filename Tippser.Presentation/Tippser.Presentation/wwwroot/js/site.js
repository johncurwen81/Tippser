function showComponent (id) {
    const element = document.getElementById(id);
    if (element) {
        element.style = '';
    }
}




//const navigate = (move, btnText) => {

//    document.getElementById('tab').value = btnText;

//    let tabs = Array.from(document.getElementsByTagName('tab'));
//    let buttons = Array.from(document.getElementsByTagName('a'));

//    let activeTab = 0;
    
//    const moveAmount = parseInt(move);

//    tabs.forEach((elm) => {
//        if (elm.classList.contains('tab-active')) {
//            activeTab = parseInt(elm.id.replace('tab-', ''));
//        }
//        elm.classList.remove('tab-active');
//        elm.classList.remove('tab-hidden-l');
//        elm.classList.remove('tab-hidden-r');

//    });

//    let prevTab = activeTab;

//    buttons.forEach((elm) => {
//        elm.classList.remove('btn-active');
//    });

//    if (moveAmount) {
//        activeTab += moveAmount;
//    }
//    else {
//        activeTab = parseInt(move.replace('tab-', ''));
//    }

//    if (activeTab === 0) {
//        activeTab = 1;
//    }

//    if (activeTab === 4) {
//        activeTab = 3;
//    }

//    tabs.forEach((elm) => {
//        if (parseInt(elm.id.replace('tab-','')) < activeTab) {
//            elm.classList.add('tab-hidden-l');
//        }
//        else if (parseInt(elm.id.replace('tab-', '')) > activeTab) {
//            elm.classList.add('tab-hidden-r');
//        }
//    });

//    document.getElementById(`nav-tab-${activeTab}`).classList.add('btn-active');
//    document.getElementById(`tab-${activeTab}`).classList.add('tab-active');
//}

//const navigateTables = (move) => {

//    let tabs = Array.from(document.getElementsByClassName('tab-part'));

//    tabs.forEach((elm) => {
//        elm.classList.remove('tab-part-active');
//        elm.classList.add('tab-part-hidden-l');
//    });

//    document.getElementById(move).classList.add('tab-part-active');
//    document.getElementById(move).classList.remove('tab-part-hidden-l');

//}
