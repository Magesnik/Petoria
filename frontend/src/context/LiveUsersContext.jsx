import React, { createContext, useState, useEffect, useContext } from 'react';
import { getBaseUrl } from '../utils/api';
import * as signalR from '@microsoft/signalr';

const LiveUsersContext = createContext();

export const LiveUsersProvider = ({ children }) => {
    const [liveUsers, setLiveUsers] = useState(0);

    useEffect(() => {
        let isMounted = true;
        // Setup global SignalR connection
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${getBaseUrl()}/hubs/liveusers`)
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Error)
            .build();

        newConnection.on("UpdateUserCount", (count) => {
            if (isMounted) {
                setLiveUsers(count);
            }
        });

        const startConnection = async () => {
            try {
                if (newConnection.state === signalR.HubConnectionState.Disconnected && isMounted) {
                    await newConnection.start();
                }
            } catch (err) {
                if (err.name !== 'AbortError') {
                    console.error('SignalR Connection Error: ', err);
                }
            }
        };

        startConnection();

        return () => {
            isMounted = false;
            if (newConnection && newConnection.state !== signalR.HubConnectionState.Disconnected) {
                // Background stop without await to prevent blocking unmount
                newConnection.stop().catch(e => console.error("SignalR stop error", e));
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
