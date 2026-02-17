import React, { useState } from 'react';
import { api } from '../utils/api';
import { useLanguage } from '../context/LanguageContext';

import './Support.css';

const Support = () => {
    const { t } = useLanguage();
    const [formData, setFormData] = useState({
        subject: '',
        message: '',
    });
    const [submitMessage, setSubmitMessage] = useState('');
    const [openFaq, setOpenFaq] = useState(null);

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value,
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            await api.post('/support/messages', formData);

            if (!response.ok) {
                throw new Error('Failed to send message');
            }

            setSubmitMessage(t('messageSent'));
            setFormData({
                subject: '',
                message: '',
            });
            setTimeout(() => setSubmitMessage(''), 5000);
        } catch (error) {
            console.error('Error sending support message:', error);
            // Optional: set error state
        }
    };

    const toggleFaq = (index) => {
        setOpenFaq(openFaq === index ? null : index);
    };

    const faqs = [
        {
            question: t('faq1Q'),
            answer: t('faq1A'),
        },
        {
            question: t('faq2Q'),
            answer: t('faq2A'),
        },
        {
            question: t('faq3Q'),
            answer: t('faq3A'),
        },
        {
            question: t('faq4Q'),
            answer: t('faq4A'),
        },
        {
            question: t('faq5Q'),
            answer: t('faq5A'),
        },
    ];

    return (
        <>

            <div className="support-page">
                <div className="support-hero">
                    <h1>{t('supportTitle')}</h1>
                    <p>{t('supportSubtitle')}</p>
                </div>

                <div className="support-container">
                    {/* Contact Form */}
                    <div className="support-section">
                        <h2>{t('contactSupportForm')}</h2>
                        <form onSubmit={handleSubmit} className="support-form">
                            <div className="form-group">
                                <label htmlFor="subject">{t('subject')}</label>
                                <input
                                    type="text"
                                    id="subject"
                                    name="subject"
                                    value={formData.subject}
                                    onChange={handleChange}
                                    required
                                    placeholder={t('subject')}
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="message">{t('yourMessage')}</label>
                                <textarea
                                    id="message"
                                    name="message"
                                    value={formData.message}
                                    onChange={handleChange}
                                    required
                                    rows="6"
                                    placeholder={t('yourMessage')}
                                />
                            </div>
                            {submitMessage && <div className="success-message">{submitMessage}</div>}
                            <button type="submit" className="btn btn-primary">
                                {t('sendMessage')}
                            </button>
                        </form>
                    </div>

                    {/* FAQ Section */}
                    <div className="support-section">
                        <h2>{t('faqTitle')}</h2>
                        <div className="faq-list">
                            {faqs.map((faq, index) => (
                                <div key={index} className={`faq-item ${openFaq === index ? 'open' : ''}`}>
                                    <button className="faq-question" onClick={() => toggleFaq(index)}>
                                        <span>{faq.question}</span>
                                        <span className="faq-icon">{openFaq === index ? '−' : '+'}</span>
                                    </button>
                                    {openFaq === index && (
                                        <div className="faq-answer">
                                            <p>{faq.answer}</p>
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* Contact Info */}
                    <div className="contact-cards">
                        <div className="contact-card">
                            <div className="contact-icon">📧</div>
                            <h3>{t('email')}</h3>
                            <p>support@petoria.com</p>
                        </div>
                        <div className="contact-card">
                            <div className="contact-icon">📞</div>
                            <h3>{t('phone')}</h3>
                            <p>+1 (555) 123-4567</p>
                        </div>
                        <div className="contact-card">
                            <div className="contact-icon">💬</div>
                            <h3>{t('liveChat')}</h3>
                            <p>{t('available24_7')}</p>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
};

export default Support;
