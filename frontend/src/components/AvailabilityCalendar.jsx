import React, { useState, useEffect } from 'react';
import './AvailabilityCalendar.css';

const AvailabilityCalendar = ({ hotelId, roomTypes }) => {
    const [currentMonth, setCurrentMonth] = useState(new Date());
    const [availability, setAvailability] = useState([]);
    const [selectedRoomType, setSelectedRoomType] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    // Selection state for bulk edit
    const [selectionStart, setSelectionStart] = useState(null);
    const [selectionEnd, setSelectionEnd] = useState(null);
    const [showBulkEdit, setShowBulkEdit] = useState(false);
    const [bulkEditCount, setBulkEditCount] = useState(0);
    const [discountPercentage, setDiscountPercentage] = useState(0);

    useEffect(() => {
        if (roomTypes.length > 0 && !selectedRoomType) {
            setSelectedRoomType(roomTypes[0]);
        }
    }, [roomTypes]);

    useEffect(() => {
        if (selectedRoomType) {
            fetchAvailability();
        }
    }, [selectedRoomType, currentMonth]);

    const fetchAvailability = async () => {
        setLoading(true);
        try {
            const startOfMonth = new Date(currentMonth.getFullYear(), currentMonth.getMonth(), 1);
            const endOfMonth = new Date(currentMonth.getFullYear(), currentMonth.getMonth() + 2, 0);

            const fromStr = startOfMonth.toISOString().split('T')[0];
            const toStr = endOfMonth.toISOString().split('T')[0];

            const response = await fetch(
                `http://localhost:5150/api/hotels/${hotelId}/availability?from=${fromStr}&to=${toStr}`
            );

            if (response.ok) {
                const data = await response.json();
                setAvailability(data);
            }
        } catch (err) {
            console.error('Error fetching availability:', err);
        } finally {
            setLoading(false);
        }
    };

    const getAvailabilityForDate = (date) => {
        if (!selectedRoomType) return null;
        const dateStr = date.toISOString().split('T')[0];
        return availability.find(
            a => a.roomTypeId === selectedRoomType.id && a.date.split('T')[0] === dateStr
        );
    };

    const handleDayClick = (date) => {
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        if (date < today) return; // Don't allow selecting past dates

        if (!selectionStart) {
            setSelectionStart(date);
            setSelectionEnd(date);
        } else if (!selectionEnd || selectionStart.getTime() === selectionEnd.getTime()) {
            if (date < selectionStart) {
                setSelectionEnd(selectionStart);
                setSelectionStart(date);
            } else {
                setSelectionEnd(date);
            }
            setShowBulkEdit(true);

            // Get current count and discount for selected date range
            const avail = getAvailabilityForDate(selectionStart);
            setBulkEditCount(avail?.availableCount ?? selectedRoomType?.totalRooms ?? 0);
            setDiscountPercentage(avail?.discountPercentage ?? 0);
        } else {
            // Reset selection
            setSelectionStart(date);
            setSelectionEnd(date);
            setShowBulkEdit(false);
        }
    };

    const isDateInSelection = (date) => {
        if (!selectionStart || !selectionEnd) return false;
        return date >= selectionStart && date <= selectionEnd;
    };

    const handleBulkUpdate = async () => {
        if (!selectionStart || !selectionEnd || !selectedRoomType) return;

        setError('');
        setSuccess('');

        try {
            const token = localStorage.getItem('token');

            // Update availability
            const response = await fetch(
                `http://localhost:5150/api/hotels/${hotelId}/availability/bulk`,
                {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${token}`
                    },
                    body: JSON.stringify({
                        roomTypeId: selectedRoomType.id,
                        startDate: selectionStart.toISOString(),
                        endDate: selectionEnd.toISOString(),
                        availableCount: bulkEditCount
                    })
                }
            );

            if (!response.ok) throw new Error('Failed to update');

            // If discount percentage is set, create/update discount
            if (discountPercentage > 0) {
                const discountResponse = await fetch(
                    `http://localhost:5150/api/hotels/${hotelId}/discounts`,
                    {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer ${token}`
                        },
                        body: JSON.stringify({
                            roomTypeId: selectedRoomType.id,
                            startDate: selectionStart.toISOString(),
                            endDate: selectionEnd.toISOString(),
                            discountPercentage: discountPercentage
                        })
                    }
                );

                if (!discountResponse.ok) {
                    const errorData = await discountResponse.json();
                    throw new Error(errorData.message || 'Failed to create discount');
                }
            }

            setSuccess(discountPercentage > 0
                ? 'Наличността и отстъпката са обновени успешно!'
                : 'Наличността е обновена успешно!');
            setShowBulkEdit(false);
            setSelectionStart(null);
            setSelectionEnd(null);
            setDiscountPercentage(0);
            fetchAvailability();
        } catch (err) {
            setError(err.message || 'Грешка при обновяване');
        }
    };

    const handleBlockDates = async (block) => {
        if (!selectionStart || !selectionEnd) return;

        setError('');
        setSuccess('');

        try {
            const token = localStorage.getItem('token');
            const response = await fetch(
                `http://localhost:5150/api/hotels/${hotelId}/availability/block`,
                {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${token}`
                    },
                    body: JSON.stringify({
                        roomTypeId: selectedRoomType?.id,
                        startDate: selectionStart.toISOString(),
                        endDate: selectionEnd.toISOString(),
                        isBlocked: block
                    })
                }
            );

            if (!response.ok) throw new Error('Failed to update');

            setSuccess(block ? 'Датите са блокирани!' : 'Датите са отблокирани!');
            setShowBulkEdit(false);
            setSelectionStart(null);
            setSelectionEnd(null);
            fetchAvailability();
        } catch (err) {
            setError('Грешка при блокиране');
        }
    };

    const previousMonth = () => {
        setCurrentMonth(new Date(currentMonth.getFullYear(), currentMonth.getMonth() - 1, 1));
    };

    const nextMonth = () => {
        setCurrentMonth(new Date(currentMonth.getFullYear(), currentMonth.getMonth() + 1, 1));
    };

    const getDaysInMonth = () => {
        const year = currentMonth.getFullYear();
        const month = currentMonth.getMonth();
        const firstDay = new Date(year, month, 1);
        const lastDay = new Date(year, month + 1, 0);
        const days = [];

        // Add empty days for padding
        for (let i = 0; i < firstDay.getDay(); i++) {
            days.push(null);
        }

        // Add days of month
        for (let i = 1; i <= lastDay.getDate(); i++) {
            days.push(new Date(year, month, i));
        }

        return days;
    };

    const formatMonth = (date) => {
        const months = ['Януари', 'Февруари', 'Март', 'Април', 'Май', 'Юни',
            'Юли', 'Август', 'Септември', 'Октомври', 'Ноември', 'Декември'];
        return `${months[date.getMonth()]} ${date.getFullYear()}`;
    };

    if (roomTypes.length === 0) {
        return (
            <div className="availability-calendar empty">
                <h3>📅 Календар за наличност</h3>
                <p className="empty-message">
                    Моля, първо добавете типове стаи от таба "Стаи".
                </p>
            </div>
        );
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return (
        <div className="availability-calendar">
            <h3>📅 Календар за наличност</h3>

            {error && <div className="calendar-error">{error}</div>}
            {success && <div className="calendar-success">{success}</div>}

            {/* Room Type Selector */}
            <div className="room-type-selector">
                <label>Тип стая:</label>
                <div className="room-type-buttons">
                    {roomTypes.map(room => (
                        <button
                            key={room.id}
                            className={`room-type-btn ${selectedRoomType?.id === room.id ? 'active' : ''}`}
                            onClick={() => {
                                setSelectedRoomType(room);
                                setSelectionStart(null);
                                setSelectionEnd(null);
                                setShowBulkEdit(false);
                            }}
                        >
                            {room.name}
                            <span className="room-count">({room.totalRooms} стаи)</span>
                        </button>
                    ))}
                </div>
            </div>

            {/* Calendar Navigation */}
            <div className="calendar-nav">
                <button onClick={previousMonth}>‹</button>
                <span className="current-month">{formatMonth(currentMonth)}</span>
                <button onClick={nextMonth}>›</button>
            </div>

            {/* Calendar Grid */}
            <div className="calendar-grid">
                <div className="day-header">Нд</div>
                <div className="day-header">Пн</div>
                <div className="day-header">Вт</div>
                <div className="day-header">Ср</div>
                <div className="day-header">Чт</div>
                <div className="day-header">Пт</div>
                <div className="day-header">Сб</div>

                {getDaysInMonth().map((date, index) => {
                    if (!date) {
                        return <div key={index} className="day-cell empty"></div>;
                    }

                    const avail = getAvailabilityForDate(date);
                    const isPast = date < today;
                    const isSelected = isDateInSelection(date);
                    const isBlocked = avail?.isBlocked;
                    const availableCount = avail?.availableCount ?? selectedRoomType?.totalRooms ?? 0;
                    const isFull = availableCount === 0;
                    const hasDiscount = avail?.discountPercentage > 0;

                    return (
                        <div
                            key={index}
                            className={`day-cell 
                                ${isPast ? 'past' : ''} 
                                ${isSelected ? 'selected' : ''} 
                                ${isBlocked ? 'blocked' : ''}
                                ${isFull && !isBlocked ? 'full' : ''}
                                ${availableCount > 0 && !isBlocked ? 'available' : ''}
                                ${hasDiscount && availableCount > 0 && !isBlocked ? 'discounted' : ''}
                            `}
                            onClick={() => !isPast && handleDayClick(date)}
                        >
                            {hasDiscount && !isPast && !isBlocked && (
                                <span className="discount-badge">-{avail.discountPercentage}%</span>
                            )}
                            <span className="day-number">{date.getDate()}</span>
                            {!isPast && selectedRoomType && (
                                <span className="day-count">
                                    {isBlocked ? '🚫' : availableCount}
                                </span>
                            )}
                        </div>
                    );
                })}
            </div>

            {/* Legend */}
            <div className="calendar-legend">
                <div className="legend-item">
                    <span className="legend-color available"></span>
                    <span>Налични стаи</span>
                </div>
                <div className="legend-item">
                    <span className="legend-color discounted"></span>
                    <span>С отстъпка</span>
                </div>
                <div className="legend-item">
                    <span className="legend-color full"></span>
                    <span>Пълно</span>
                </div>
                <div className="legend-item">
                    <span className="legend-color blocked"></span>
                    <span>Блокирано</span>
                </div>
            </div>

            {/* Bulk Edit Panel */}
            {showBulkEdit && selectionStart && selectionEnd && (
                <div className="bulk-edit-panel">
                    <h4>
                        Редактиране: {selectionStart.toLocaleDateString('bg-BG')} - {selectionEnd.toLocaleDateString('bg-BG')}
                    </h4>

                    <div className="bulk-edit-form">
                        <div className="form-group">
                            <label>Брой налични стаи:</label>
                            <div className="count-selector">
                                <button
                                    onClick={() => setBulkEditCount(Math.max(0, bulkEditCount - 1))}
                                    disabled={bulkEditCount <= 0}
                                >
                                    −
                                </button>
                                <input
                                    type="number"
                                    value={bulkEditCount}
                                    onChange={(e) => setBulkEditCount(Math.max(0, parseInt(e.target.value) || 0))}
                                    min="0"
                                    max={selectedRoomType?.totalRooms || 100}
                                />
                                <button
                                    onClick={() => setBulkEditCount(bulkEditCount + 1)}
                                    disabled={bulkEditCount >= (selectedRoomType?.totalRooms || 100)}
                                >
                                    +
                                </button>
                            </div>
                            <small>Максимум: {selectedRoomType?.totalRooms} стаи</small>
                        </div>

                        <div className="form-group">
                            <label>Отстъпка (%):</label>
                            <div className="count-selector">
                                <button
                                    onClick={() => setDiscountPercentage(Math.max(0, discountPercentage - 5))}
                                    disabled={discountPercentage <= 0}
                                >
                                    −
                                </button>
                                <input
                                    type="number"
                                    value={discountPercentage}
                                    onChange={(e) => setDiscountPercentage(Math.min(99, Math.max(0, parseInt(e.target.value) || 0)))}
                                    min="0"
                                    max="99"
                                />
                                <button
                                    onClick={() => setDiscountPercentage(Math.min(99, discountPercentage + 5))}
                                    disabled={discountPercentage >= 99}
                                >
                                    +
                                </button>
                            </div>
                            <small>0% = без отстъпка, максимум 99%</small>
                        </div>

                        <div className="bulk-actions">
                            <button className="btn-update" onClick={handleBulkUpdate}>
                                ✓ Запази
                            </button>
                            <button className="btn-block" onClick={() => handleBlockDates(true)}>
                                🚫 Блокирай
                            </button>
                            <button className="btn-unblock" onClick={() => handleBlockDates(false)}>
                                ✓ Отблокирай
                            </button>
                            <button className="btn-cancel" onClick={() => {
                                setShowBulkEdit(false);
                                setSelectionStart(null);
                                setSelectionEnd(null);
                            }}>
                                Отказ
                            </button>
                        </div>
                    </div>
                </div>
            )}

            <div className="calendar-instructions">
                <p>💡 Кликнете върху две дати, за да изберете период и да зададете наличност.</p>
            </div>
        </div>
    );
};

export default AvailabilityCalendar;
