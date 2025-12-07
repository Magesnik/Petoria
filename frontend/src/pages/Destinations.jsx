import React from 'react';
import Header from '../components/Header';
import './Home.css';

const Destinations = () => {
    return (
        <div className="home-page">
            <Header />
            <div className="hero-section">
                <div className="hero-content">
                    <h1>Дестинации</h1>
                    <p>Скоро...</p>
                </div>
            </div>
        </div>
    );
};

export default Destinations;
