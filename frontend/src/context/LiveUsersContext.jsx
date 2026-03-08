import React, { createContext, useState, useEffect, useContext } from 'react';
import { getBaseUrl } from '../utils/api';
import * as signalR from '@microsoft/signalr';

const LiveUsersContext = createContext();

export const LiveUsersProvider = ({ children }) => {
    const [liveUsers, setLiveUsers] = useState(0);

    useEffect(() => {
        // Setup global SignalR connection
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${getBaseUrl()}/hubs/liveusers`)
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Error)
            .build();

        newConnection.on("UpdateUserCount", (count) => {
            setLiveUsers(count);
        });

        newConnection.start()
            .catch(err => console.error('SignalR Connection Error: ', err));

        return () => {
            if (newConnection) {
                newConnection.stop();
            }
        };
    }, []);

    return (
        <LiveUsersContext.Provider value={{ liveUsers }}>
            {children}
        </LiveUsersContext.Provider>
    );
};

export const useLiveUsers = () => useContext(LiveUsersContext);
