import React, { useState } from 'react';
import { api } from '../../utils/api';
import { useLanguage } from '../../context/LanguageContext';

import './Support.css';

/** Страница за поддръжка с форма за съобщения и секция с често задавани въпроси. */
const Support = () => {
    const { t } = useLanguage();
    // Данни от формуляра за поддръжка
    const [formData, setFormData] = useState({
        subject: '',
        message: '',
    });
    const [submitMessage, setSubmitMessage] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    // Индекс на отворения FAQ въпрос
    const [openFaq, setOpenFaq] = useState(null);

    // Обновява полетата на формуляра при промяна
    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value,
        });
        setErrorMessage('');
    };

    // Изпраща съобщение до поддръжката
    const handleSubmit = async (e) => {
        e.preventDefault();
        setErrorMessage('');

        try {
            await api.post('/support/messages', formData);

            setSubmitMessage(t('messageSent'));
            setFormData({
                subject: '',
                message: '',
            });
            setTimeout(() => setSubmitMessage(''), 5000);
        } catch (error) {
            console.error('Error sending support message:', error);
            if (error.message?.includes("10 unanswered messages")) {
                setErrorMessage(t('messageLimitReachedError'));
            } else {
                setErrorMessage(t('error') || 'Error sending message');
            }
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
                                    minLength="10"
                                    rows="6"
                                    placeholder={t('yourMessage')}
                                />
                                <small className="text-muted" style={{ display: 'block', marginTop: '5px' }}>{t('messageMinLengthInfo')}</small>
                            </div>
                            {errorMessage && <div className="error-message">{errorMessage}</div>}
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
                </div>
            </div>
        </>
    );
};

export default Support;
