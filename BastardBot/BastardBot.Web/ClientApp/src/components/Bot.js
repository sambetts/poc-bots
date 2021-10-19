import React, { useMemo } from 'react';
import ReactWebChat, { createDirectLine, createStore } from 'botframework-webchat';
import "./Bot.css";
import botAvatar from '../img/robotavatar.png';

// Hack: https://github.com/microsoft/BotFramework-WebChat/blob/master/packages/core/src/utils/mime-wrapper.js#L40

export default () => {
    const directLine = useMemo(() => createDirectLine({ token: 'MEJmt6iEd8A.Grs-xxyGtyMAhtg_Z2_M0PWSJwBKkmqtYiGUSCkDtpk' }), []);


    const store = createStore({}, ({ dispatch }) => next => action => {
        if (action.type === 'DIRECT_LINE/CONNECT_FULFILLED') {
            dispatch({
                type: 'WEB_CHAT/SEND_EVENT',
                payload: {
                    name: 'webchat/join',
                    value: { language: window.navigator.language }
                }
            });
        }

        return next(action);
    });

    const styleOptions = {
        userAvatarInitials: 'You',
        hideUploadButton: true,
        botAvatarImage: botAvatar,
        botAvatarBackgroundColor: "#FFFFFF"
    };


    return <div id="chatWindow"><ReactWebChat directLine={directLine} userID="YOUR_USER_ID" store={store} styleOptions={styleOptions} /></div>;
};
