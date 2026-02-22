import React, { useEffect, useState } from 'react';
import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet';
import MarkerClusterGroup from 'react-leaflet-cluster';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import './HotelMap.css';

// Fix для default icons в Leaflet
import icon from 'leaflet/dist/images/marker-icon.png';
import iconShadow from 'leaflet/dist/images/marker-shadow.png';

let DefaultIcon = L.icon({
    iconUrl: icon,
    shadowUrl: iconShadow,
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41]
});

L.Marker.prototype.options.icon = DefaultIcon;

// Component to fit bounds when hotels change
const FitBounds = ({ hotels }) => {
    const map = useMap();

    useEffect(() => {
        if (hotels && hotels.length > 0) {
            const validHotels = hotels.filter(h =>
                h.latitude != null &&
                h.longitude != null &&
                h.latitude !== 0 &&
                h.longitude !== 0
            );

            if (validHotels.length > 0) {
                const bounds = L.latLngBounds(
                    validHotels.map(hotel => [hotel.latitude, hotel.longitude])
                );
                map.fitBounds(bounds, { padding: [50, 50], maxZoom: 13 });
            }
        }
    }, [hotels, map]);

    return null;
};

const HotelMap = ({ hotels, onHotelClick }) => {
    const [mapReady, setMapReady] = useState(false);

    // Filter hotels with valid coordinates
    const validHotels = hotels.filter(
        hotel =>
            hotel.latitude != null &&
            hotel.longitude != null &&
            hotel.latitude !== 0 &&
            hotel.longitude !== 0
    );

    // Default center (Sofia, Bulgaria)
    const defaultCenter = [42.6977, 23.3219];
    const defaultZoom = 6;

    const handleViewDetails = (hotelId) => {
        if (onHotelClick) {
            onHotelClick(hotelId);
        } else {
            // Navigate to hotel details page
            window.location.href = `/hotel/${hotelId}`;
        }
    };

    return (
        <div className="hotel-map-container">
            {validHotels.length === 0 ? (
                <div className="map-empty-state">
                    <h3>📍 Няма хотели с координати</h3>
                    <p>Хотелите в базата данни нямат географски координати или всички са филтрирани.</p>
                </div>
            ) : (
                <MapContainer
                    center={defaultCenter}
                    zoom={defaultZoom}
                    minZoom={2}
                    maxBounds={[[-90, -180], [90, 180]]}
                    className="leaflet-map"
                    whenReady={() => setMapReady(true)}
                >
                    <TileLayer
                        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                        noWrap={true}
                        bounds={[[-90, -180], [90, 180]]}
                    />

                    <MarkerClusterGroup
                        chunkedLoading
                        showCoverageOnHover={false}
                        spiderfyOnMaxZoom={true}
                        maxClusterRadius={60}
                    >
                        {validHotels.map((hotel) => (
                            <Marker
                                key={hotel.id}
                                position={[hotel.latitude, hotel.longitude]}
                            >
                                <Popup className="hotel-popup">
                                    <div className="popup-content">
                                        {hotel.imageUrl && (
                                            <img
                                                src={hotel.imageUrl}
                                                alt={hotel.name}
                                                className="popup-image"
                                            />
                                        )}
                                        <h3 className="popup-title">{hotel.name}</h3>
                                        <p className="popup-location">
                                            📍 {hotel.city}, {hotel.country}
                                        </p>
                                        <div className="popup-details">
                                            <div className="popup-price">
                                                <span className="popup-price-label">Цена:</span>
                                                <span className="popup-price-value">
                                                    ${hotel.pricePerNight}
                                                </span>
                                                <span className="popup-price-night">/нощувка</span>
                                            </div>
                                            <div className="popup-rating">
                                                <span className="rating-stars">⭐</span>
                                                <span className="rating-value">{hotel.rating.toFixed(1)}</span>
                                            </div>
                                        </div>
                                        <button
                                            className="popup-button"
                                            onClick={() => handleViewDetails(hotel.id)}
                                        >
                                            View Details
                                        </button>
                                    </div>
                                </Popup>
                            </Marker>
                        ))}
                    </MarkerClusterGroup>

                    {mapReady && <FitBounds hotels={validHotels} />}
                </MapContainer>
            )}
        </div>
    );
};

export default HotelMap;
