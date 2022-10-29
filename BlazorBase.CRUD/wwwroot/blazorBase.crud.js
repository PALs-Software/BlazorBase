class blazorbaseDataListInput {
    static dotNetReferences = new Object();
    static listActive;

    static init(id, dotNetReference, resetValueAfterSelection) {
        document.getElementById(id).addEventListener('focusin', blazorbaseDataListInput.listShow);
        document.getElementById(id).addEventListener('focusout', blazorbaseDataListInput.onFocusOut);

        blazorbaseDataListInput.dotNetReferences[id] = { dotNetReference: dotNetReference, resetValueAfterSelection: resetValueAfterSelection, isSelfNavigating: false };
    }

    static listShow(e) {

        const input = blazorbaseDataListInput.target(e);
        if (!input) return;

        if (input.list) {

            // setup of datalist control
            const dl = input.list;
            input.datalist = dl;
            input.removeAttribute('list');

            dl.input = input;
            dl.setAttribute('tabindex', -1);

            // event handlers
            input.addEventListener('input', blazorbaseDataListInput.listLimit);
            input.addEventListener('keydown', blazorbaseDataListInput.listControl);
            dl.addEventListener('keydown', blazorbaseDataListInput.listKey);
            dl.addEventListener('click', blazorbaseDataListInput.listSet);
            dl.addEventListener('focusout', blazorbaseDataListInput.onFocusOut);
        }

        // show datalist
        const dl = input.datalist;
        if (dl && !dl.shown) {

            blazorbaseDataListInput.listHide(blazorbaseDataListInput.listActive);

            dl.shown = true;
            blazorbaseDataListInput.listLimit(e);
            if (input.offsetWidth < 230)
                dl.style.width = '230px';
            else
                dl.style.width = input.offsetWidth + 'px';

            dl.style.left = input.offsetLeft + 'px';
            dl.style.display = 'block';
            blazorbaseDataListInput.listActive = dl;

        }

    }

    static onFocusOut(e) {
        const element = blazorbaseDataListInput.target(e);
        if (!element)
            return;

        let input, datalist;
        if (element.tagName === "INPUT") {
            input = element;
            datalist = input.datalist;
        } else if (element.tagName === "DATALIST") {
            datalist = element;
            input = datalist.previousElementSibling;
        } else if (element.tagName === "OPTION") {
            datalist = element.parentElement;
            input = datalist.previousElementSibling;
        } else {
            return;
        }

        if (blazorbaseDataListInput.dotNetReferences[input.id].isSelfNavigating) {
            blazorbaseDataListInput.dotNetReferences[input.id].isSelfNavigating = false;
            return;
        }

        setTimeout(function () { // Wait for new focus, so active element can be checked
            if (document.activeElement &&
                (
                    document.activeElement == input ||
                    document.activeElement == datalist ||
                    (document.activeElement.parentElement && document.activeElement.parentElement == datalist)
                ))
                return;

            blazorbaseDataListInput.listHide(input.datalist);
        }, 2);
    }

    // hide datalist
    static listHide(dl) {

        if (dl && dl.shown) {
            dl.style.display = 'none';
            dl.shown = false;
        }

    }

    // enable valid and disable invalid options
    static listLimit(e) {

        const input = blazorbaseDataListInput.target(e);
        if (!input || !input.datalist) return;

        if (!input.datalist.shown)
            blazorbaseDataListInput.listShow({ target: input });

        const v = input.value.trim().toLowerCase();
        const valueArr = v.split(' ');
        Array.from(input.datalist.getElementsByTagName('option')).forEach(opt => {
            opt.setAttribute('tabindex', 0);
            opt.style.display = !v || blazorbaseDataListInput.valueContainsAllSearchArrValues(valueArr, opt.getAttribute('label').toLowerCase()) ? 'block' : 'none';
        });

    }

    static valueContainsAllSearchArrValues(searchValueArr, value) {
        for (let i = 0; i < searchValueArr.length; i++) {
            if (!value.includes(searchValueArr[i])) {
                return false;
            }
        }
        return true;
    }

    // key event on input
    static listControl(e) {

        const input = blazorbaseDataListInput.target(e);
        if (!input || !input.datalist) return;

        switch (e.keyCode) {

            case 40: {
                // arrow down
                let opt = input.datalist.firstElementChild;
                if (!opt.offsetHeight) opt = blazorbaseDataListInput.visibleSibling(opt, 1);
                if (opt) {
                    blazorbaseDataListInput.dotNetReferences[input.id].isSelfNavigating = true;
                    opt.focus();
                }
                break;
            }

            case 9:   // tab
                blazorbaseDataListInput.listHide(input.datalist);
                break;

            case 13:  // enter
                blazorbaseDataListInput.listSet(e);
                break;

            case 27:  // esc
                blazorbaseDataListInput.listHide(input.datalist);
                break;

        }

    }

    // key event on datalist
    static keymap = {
        33: -12,
        34: 12,
        38: -1,
        40: 1
    };

    static listKey(e) {
        const t = blazorbaseDataListInput.target(e);
        if (!t) return;

        const
            kc = e.keyCode,
            dir = blazorbaseDataListInput.keymap[kc],
            dl = t.parentElement;

        if (dir) {
            // move through list
            let opt = blazorbaseDataListInput.visibleSibling(t, dir);
            if (opt) {
                blazorbaseDataListInput.dotNetReferences[dl.input.id].isSelfNavigating = true;
                opt.focus();
            }
            e.preventDefault();

        }
        else if (kc === 9 || kc === 13 || kc === 32) {

            // tab, enter, space: use value
            blazorbaseDataListInput.listSet(e);

        }
        else if (kc === 8) {

            // backspace: return to input
            dl.input.focus();

        }
        else if (kc === 27) {

            // esc: hide list
            blazorbaseDataListInput.listHide(dl);

        }
    }

    // get previous/next visible sibling
    static visibleSibling(opt, dir) {

        let newOpt = opt;

        do {

            if (dir < 0) {
                newOpt = newOpt.previousElementSibling;
            }
            else if (dir > 0) {
                newOpt = newOpt.nextElementSibling;
            }

            if (newOpt && newOpt.offsetHeight) {
                opt = newOpt;
                dir -= Math.sign(dir);
            }

        } while (newOpt && dir);

        return opt;

    }

    // set datalist option to input value
    static async listSet(e) {
        let t = blazorbaseDataListInput.target(e);
        let dl = t && t.parentElement;
        let input = dl.input;

        if (!dl || !dl.input) {
            // Enter without selecting a value from the drop down list
            const v = t.value.trim().toLowerCase();
            const valueArr = v.split(' ');
            let firstOpenOption = Array.from(t.datalist.getElementsByTagName('option')).find(opt => {
                return blazorbaseDataListInput.valueContainsAllSearchArrValues(valueArr, opt.getAttribute('label').toLowerCase())
            });

            if (!firstOpenOption)
                return;

            input = t;
            t = firstOpenOption;
            dl = firstOpenOption.parentElement;
        }

        input.value = (t && t.getAttribute('label')) || '';
        input.setAttribute('data-value', (t && t.value) || '');
        await blazorbaseDataListInput.dotNetReferences[input.id].dotNetReference.invokeMethodAsync('ValueChanged', (t && t.value) || '');
        blazorbaseDataListInput.listHide(dl);

        if (blazorbaseDataListInput.dotNetReferences[input.id].resetValueAfterSelection) {
            input.value = '';
        }
    }

    // fetch target node
    static target(t) {
        return t && t.target;
    }
}

window.blazorBase.crud = {
    blazorbaseDataListInput: blazorbaseDataListInput
};