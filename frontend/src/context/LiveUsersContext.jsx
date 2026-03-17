import React, { createContext, useState, useEffect, useContext } from 'react';
import { getBaseUrl } from '../utils/api';
import * as signalR from '@microsoft/signalr';

/** Контекст за брояч на онлайн потребители чрез SignalR */
const LiveUsersContext = createContext();

/** Доставчик на контекста за онлайн потребители */
export const LiveUsersProvider = ({ children }) => {
    const [liveUsers, setLiveUsers] = useState(0);

    useEffect(() => {
        let isMounted = true;
        // Създаване на глобална SignalR връзка
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${getBaseUrl()}/hubs/liveusers`)
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Error)
            .build();

        // Слушане за обновяване на броя потребители
        newConnection.on("UpdateUserCount", (count) => {
            if (isMounted) {
                setLiveUsers(count);
            }
        });

        // Стартиране на връзката
        const startConnection = async () => {
            try {
                if (newConnection.state === signalR.HubConnectionState.Disconnected && isMounted) {
                    await newConnection.start();
                }
            } catch (err) {
                if (err.name !== 'AbortError') {
                    console.error('Грешка при SignalR връзка: ', err);
                }
            }
        };

        startConnection();

        return () => {
            isMounted = false;
            if (newConnection && newConnection.state !== signalR.HubConnectionState.Disconnected) {
                // Спиране на връзката във фонов режим без await
                newConnection.stop().catch(e => console.error("Грешка при спиране на SignalR", e));
            }
        };
    }, []);

    return (
        <LiveUsersContext.Provider value={{ liveUsers }}>
            {children}
        </LiveUsersContext.Provider>
    );
};

/** Хук за достъп до броя на онлайн потребители */
export const useLiveUsers = () => useContext(LiveUsersContext);
