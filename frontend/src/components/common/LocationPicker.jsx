import React, { useState, useEffect, useRef, useCallback } from 'react';
import { MapContainer, TileLayer, Marker, useMapEvents, useMap } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { useLanguage } from '../../context/LanguageContext';
import './LocationPicker.css';

// Icon paths
import icon from 'leaflet/dist/images/marker-icon.png';
import iconShadow from 'leaflet/dist/images/marker-shadow.png';

// Component to handle map clicks
const MapClickHandler = ({ onLocationSelect }) => {
    useMapEvents({
        click(e) {
            const { lat, lng } = e.latlng;
            onLocationSelect(lat, lng);
        }
    });
    return null;
};

// Component to update map view when coordinates change
const MapViewController = ({ center }) => {
    const map = useMap();

    useEffect(() => {
        if (center) {
            // Use setView but don't animate to avoid interrupting wheel zooms
            map.setView(center, map.getZoom(), { animate: false });
        }
    }, [center, map]);

    return null;
};

const LocationPicker = ({ onLocationSelect, initialLat = null, initialLng = null }) => {
    const { t } = useLanguage();

    const defaultIcon = React.useMemo(() => {
        return L.icon({
            iconUrl: icon,
            shadowUrl: iconShadow,
            iconSize: [25, 41],
            iconAnchor: [12, 41],
            popupAnchor: [1, -34],
            shadowSize: [41, 41]
        });
    }, []);
    // Default to Sofia, Bulgaria if no initial coordinates
    const defaultCenter = [42.6977, 23.3219];
    const [position, setPosition] = useState(
        initialLat && initialLng ? [initialLat, initialLng] : null
    );
    const [mapCenter, setMapCenter] = useState(
        initialLat && initialLng ? [initialLat, initialLng] : defaultCenter
    );
    const [addressInfo, setAddressInfo] = useState(null);
    const [loading, setLoading] = useState(false);
    const geocodeTimer = useRef(null);

    // Reverse geocoding using BigDataCloud free API (no auth, no rate limit issues)
    const reverseGeocode = useCallback(async (lat, lng) => {
        setLoading(true);
        try {
            const response = await fetch(
                `https://api.bigdatacloud.net/data/reverse-geocode-client?latitude=${lat}&longitude=${lng}&localityLanguage=en`
            );
            if (!response.ok) throw new Error('Geocoding request failed');
            const data = await response.json();

            const city = data.city || data.locality || data.principalSubdivision || '';
            const country = data.countryName || '';

            const info = { city, country, address: '' };
            setAddressInfo(info);
            return info;
        } catch (error) {
            console.error('Reverse geocoding error:', error);
            return null;
        } finally {
            setLoading(false);
        }
    }, []);

    // Debounced geocode + location select — max 1 request per second (Nominatim limit)
    const debouncedGeocode = useCallback((lat, lng) => {
        if (geocodeTimer.current) clearTimeout(geocodeTimer.current);
        geocodeTimer.current = setTimeout(async () => {
            const info = await reverseGeocode(lat, lng);
            onLocationSelect({
                lat,
                lng,
                city: info?.city || '',
                country: info?.country || '',
                address: info?.address || ''
            });
        }, 1000);
    }, [reverseGeocode, onLocationSelect]);

    const handleMapClick = (lat, lng) => {
        setPosition([lat, lng]);
        debouncedGeocode(lat, lng);
    };

    const handleMarkerDrag = (e) => {
        const { lat, lng } = e.target.getLatLng();
        setPosition([lat, lng]);
        debouncedGeocode(lat, lng);
    };

    // Update position when initial coordinates change from outside
    useEffect(() => {
        if (initialLat && initialLng) {
            // Only update if significantly different to prevent feedback loop 
            // from our own onLocationSelect calls
            if (!position ||
                Math.abs(position[0] - initialLat) > 0.0001 ||
                Math.abs(position[1] - initialLng) > 0.0001) {
                const newPosition = [initialLat, initialLng];
                setPosition(newPosition);
                setMapCenter(newPosition);
            }
        }
    }, [initialLat, initialLng, position]);

    return (
        <div className="location-picker">
            <div className="location-picker-instructions">
                <p>📍 {t('clickMapToSelect')}</p>
                {loading && (
                    <div className="geocoding-loading">
                        ⏳ {t('loadingAddress')}
                    </div>
                )}
                {position && !loading && (
                    <div className="coordinates-display">
                        <span className="coordinate">
                            <strong>{t('lat')}:</strong> {position[0].toFixed(6)}
                        </span>
                        <span className="coordinate">
                            <strong>{t('lng')}:</strong> {position[1].toFixed(6)}
                        </span>
                    </div>
                )}
                {addressInfo && !loading && (
                    <div className="address-display">
                        {addressInfo.address && (
                            <span className="address-item">
                                <strong>📍 {t('addressLabel')}:</strong> {addressInfo.address}
                            </span>
                        )}
                        {addressInfo.city && (
                            <span className="address-item">
                                <strong>🏙️ {t('cityLabel')}:</strong> {addressInfo.city}
                            </span>
                        )}
                        {addressInfo.country && (
                            <span className="address-item">
                                <strong>🌍 {t('countryLabel')}:</strong> {addressInfo.country}
                            </span>
                        )}
                    </div>
                )}
            </div>

            <MapContainer
                center={initialLat && initialLng ? [initialLat, initialLng] : defaultCenter}
                zoom={13}
                minZoom={2}
                maxBounds={L.latLngBounds(L.latLng(-90, -180), L.latLng(90, 180))}
                className="location-picker-map"
                scrollWheelZoom={true}
            >
                <TileLayer
                    attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                    noWrap={true}
                    bounds={L.latLngBounds(L.latLng(-90, -180), L.latLng(90, 180))}
                />

                <MapClickHandler onLocationSelect={handleMapClick} />
                <MapViewController center={mapCenter} />

                {position && (
                    <Marker
                        position={position}
                        draggable={true}
                        icon={defaultIcon}
                        eventHandlers={{
                            dragend: handleMarkerDrag
                        }}
                    />
                )}
            </MapContainer>

            {!position && (
                <div className="location-hint">
                    {t('selectLocationOrEnterCoords')}
                </div>
            )}
        </div>
    );
};

export default LocationPicker;
