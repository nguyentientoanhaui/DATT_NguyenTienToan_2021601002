// Simple Chatbot with AI Connection
(function() {
    'use strict';
    
    console.log('Chatbot script starting...');
    
    // Wait for DOM ready
    function initChatbot() {
        console.log('Initializing chatbot...');
        
        // Create chatbot HTML
        const chatbotHTML = `
            <div id="chatbot-widget" style="position: fixed; bottom: 20px; right: 20px; z-index: 1000;">
                <!-- Button -->
                <div id="chatbot-button" style="width: 50px; height: 50px; background: #000; border-radius: 50%; cursor: pointer; display: flex; align-items: center; justify-content: center; box-shadow: 0 2px 10px rgba(0,0,0,0.3);">
                    <i class="fas fa-comment" style="color: white; font-size: 18px;"></i>
                </div>
                
                <!-- Window -->
                <div id="chatbot-window" style="position: absolute; bottom: 70px; right: 0; width: 350px; height: 600px; background: white; border: 2px solid #000; border-radius: 15px; display: none; flex-direction: column; box-shadow: 0 8px 25px rgba(0,0,0,0.3);">
                    <!-- Header -->
                    <div style="background: #000; color: white; padding: 12px 15px; display: flex; justify-content: space-between; align-items: center; border-radius: 13px 13px 0 0;">
                        <div style="display: flex; align-items: center; gap: 8px;">
                            <i class="fas fa-robot"></i>
                            <span style="font-weight: bold;">Trợ lý AI</span>
                        </div>
                        <button id="chatbot-close" style="background: none; border: none; color: white; cursor: pointer; padding: 4px;">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                    
                    <!-- Messages -->
                    <div id="chatbot-messages" style="flex: 1; padding: 15px; overflow-y: auto; background: white; scroll-behavior: smooth;">
                        <div style="display: flex; gap: 8px; margin-bottom: 12px;">
                            <div style="width: 30px; height: 30px; background: #f0f0f0; border: 2px solid #000; border-radius: 50%; display: flex; align-items: center; justify-content: center;">
                                <i class="fas fa-robot" style="font-size: 14px;"></i>
                            </div>
                            <div style="flex: 1;">
                                <div style="background: white; border: 2px solid #000; padding: 10px 12px; border-radius: 15px; border-bottom-left-radius: 4px; font-size: 13px;">
                                    Xin chào! Tôi là trợ lý AI của cửa hàng. Tôi có thể giúp gì cho bạn?
                                </div>
                                <div style="font-size: 10px; color: #666; margin-top: 4px;">${getTime()}</div>
                            </div>
                        </div>
                    </div>
                    
                    <!-- Input -->
                    <div style="padding: 15px; background: white; border-top: 2px solid #000; display: flex; gap: 8px; border-radius: 0 0 13px 13px;">
                        <input type="text" id="chatbot-input" placeholder="Nhập tin nhắn..." style="flex: 1; padding: 10px 12px; border: 2px solid #000; border-radius: 20px; font-size: 13px; outline: none;">
                        <button id="chatbot-send" style="width: 35px; height: 35px; background: #000; border: 2px solid #000; border-radius: 50%; color: white; cursor: pointer; display: flex; align-items: center; justify-content: center;">
                            <i class="fas fa-paper-plane" style="font-size: 12px;"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        // Add CSS for typing animation
        const style = document.createElement('style');
        style.textContent = `
            @keyframes typing {
                0%, 60%, 100% { transform: translateY(0); }
                30% { transform: translateY(-10px); }
            }
            #chatbot-messages::-webkit-scrollbar {
                width: 6px;
            }
            #chatbot-messages::-webkit-scrollbar-track {
                background: #f1f1f1;
                border-radius: 3px;
            }
            #chatbot-messages::-webkit-scrollbar-thumb {
                background: #000;
                border-radius: 3px;
            }
            #chatbot-messages::-webkit-scrollbar-thumb:hover {
                background: #333;
            }
        `;
        document.head.appendChild(style);
        
        // Add to body
        document.body.insertAdjacentHTML('beforeend', chatbotHTML);
        console.log('Chatbot HTML added');
        
        // Get elements
        const button = document.getElementById('chatbot-button');
        const window = document.getElementById('chatbot-window');
        const close = document.getElementById('chatbot-close');
        const input = document.getElementById('chatbot-input');
        const send = document.getElementById('chatbot-send');
        const messages = document.getElementById('chatbot-messages');
        
        console.log('Elements found:', {button: !!button, window: !!window, close: !!close, input: !!input, send: !!send});
        
        // Button click
        button.addEventListener('click', function() {
            console.log('Button clicked!');
            const isOpen = window.style.display === 'flex';
            if (isOpen) {
                window.style.display = 'none';
                console.log('Chat closed');
            } else {
                window.style.display = 'flex';
                console.log('Chat opened');
            }
        });
        
        // Close button
        close.addEventListener('click', function() {
            console.log('Close clicked!');
            window.style.display = 'none';
        });
        
        // Send message to AI
        async function sendMessage() {
            const message = input.value.trim();
            if (message) {
                console.log('Sending to AI:', message);
                
                // Add user message
                addMessage(message, true);
                input.value = '';
                
                // Show typing indicator
                showTyping();
                
                try {
                    // Send to AI API
                    const response = await fetch('/api/chat/send', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify({
                            message: message,
                            sessionId: getSessionId()
                        })
                    });
                    
                    if (response.ok) {
                        const data = await response.json();
                        console.log('AI Response:', data);
                        
                        // Hide typing indicator
                        hideTyping();
                        
                        // Add AI response
                        addMessage(data.message || 'Xin lỗi, tôi không thể trả lời lúc này.', false);
                        
                        // Add quick replies if available
                        if (data.quickReplies && data.quickReplies.length > 0) {
                            addQuickReplies(data.quickReplies);
                        }
                    } else {
                        console.error('API Error:', response.status);
                        hideTyping();
                        addMessage('Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau.', false);
                    }
                } catch (error) {
                    console.error('Network Error:', error);
                    hideTyping();
                    addMessage('Xin lỗi, không thể kết nối với AI. Vui lòng kiểm tra kết nối mạng.', false);
                }
            }
        }
        
        // Send button
        send.addEventListener('click', sendMessage);
        
        // Enter key
        input.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                sendMessage();
            }
        });
        
        // Add message function
        function addMessage(text, isUser) {
            const messageDiv = document.createElement('div');
            messageDiv.style.cssText = 'display: flex; gap: 8px; margin-bottom: 12px; ' + (isUser ? 'flex-direction: row-reverse;' : '');
            
            messageDiv.innerHTML = `
                <div style="width: 30px; height: 30px; background: ${isUser ? '#000' : '#f0f0f0'}; border: 2px solid #000; border-radius: 50%; display: flex; align-items: center; justify-content: center;">
                    <i class="fas ${isUser ? 'fa-user' : 'fa-robot'}" style="font-size: 14px; color: ${isUser ? 'white' : 'black'};"></i>
                </div>
                <div style="flex: 1;">
                    <div style="background: ${isUser ? '#000' : 'white'}; color: ${isUser ? 'white' : 'black'}; border: 2px solid #000; padding: 10px 12px; border-radius: 15px; border-bottom-${isUser ? 'right' : 'left'}-radius: 4px; font-size: 13px;">
                        ${text}
                    </div>
                    <div style="font-size: 10px; color: #666; margin-top: 4px; text-align: ${isUser ? 'right' : 'left'};">
                        ${getTime()}
                    </div>
                </div>
            `;
            
            messages.appendChild(messageDiv);
            // Smooth scroll to bottom with delay to ensure content is rendered
            setTimeout(() => {
                messages.scrollTop = messages.scrollHeight;
            }, 100);
        }
        
        // Show typing indicator
        function showTyping() {
            const typingDiv = document.createElement('div');
            typingDiv.id = 'typing-indicator';
            typingDiv.style.cssText = 'display: flex; gap: 8px; margin-bottom: 12px;';
            typingDiv.innerHTML = `
                <div style="width: 30px; height: 30px; background: #f0f0f0; border: 2px solid #000; border-radius: 50%; display: flex; align-items: center; justify-content: center;">
                    <i class="fas fa-robot" style="font-size: 14px;"></i>
                </div>
                <div style="flex: 1;">
                    <div style="background: white; border: 2px solid #000; padding: 10px 12px; border-radius: 15px; border-bottom-left-radius: 4px; font-size: 13px; display: flex; align-items: center; gap: 4px;">
                        <div style="width: 6px; height: 6px; background: #000; border-radius: 50%; animation: typing 1.4s infinite ease-in-out;"></div>
                        <div style="width: 6px; height: 6px; background: #000; border-radius: 50%; animation: typing 1.4s infinite ease-in-out; animation-delay: 0.2s;"></div>
                        <div style="width: 6px; height: 6px; background: #000; border-radius: 50%; animation: typing 1.4s infinite ease-in-out; animation-delay: 0.4s;"></div>
                    </div>
                </div>
            `;
            
            messages.appendChild(typingDiv);
            // Smooth scroll to bottom
            setTimeout(() => {
                messages.scrollTop = messages.scrollHeight;
            }, 50);
        }
        
        // Hide typing indicator
        function hideTyping() {
            const typing = document.getElementById('typing-indicator');
            if (typing) {
                typing.remove();
            }
        }
        
        // Add quick replies
        function addQuickReplies(replies) {
            const repliesDiv = document.createElement('div');
            repliesDiv.style.cssText = 'display: flex; flex-wrap: wrap; gap: 6px; margin-top: 8px;';
            
            replies.forEach(reply => {
                const replyBtn = document.createElement('button');
                replyBtn.textContent = reply;
                replyBtn.style.cssText = 'background: white; border: 2px solid #000; color: #000; padding: 6px 10px; border-radius: 15px; font-size: 11px; cursor: pointer; transition: all 0.2s ease;';
                
                replyBtn.addEventListener('click', function() {
                    input.value = reply;
                    sendMessage();
                });
                
                replyBtn.addEventListener('mouseenter', function() {
                    this.style.background = '#000';
                    this.style.color = 'white';
                });
                
                replyBtn.addEventListener('mouseleave', function() {
                    this.style.background = 'white';
                    this.style.color = '#000';
                });
                
                repliesDiv.appendChild(replyBtn);
            });
            
            messages.appendChild(repliesDiv);
            // Smooth scroll to bottom
            setTimeout(() => {
                messages.scrollTop = messages.scrollHeight;
            }, 100);
        }
        
        // Get session ID
        function getSessionId() {
            let sessionId = localStorage.getItem('chatbot-session-id');
            if (!sessionId) {
                sessionId = 'chat_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
                localStorage.setItem('chatbot-session-id', sessionId);
            }
            return sessionId;
        }
        
        console.log('Chatbot initialized successfully!');
    }
    
    // Get current time
    function getTime() {
        const now = new Date();
        return now.getHours().toString().padStart(2, '0') + ':' + 
               now.getMinutes().toString().padStart(2, '0');
    }
    
    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initChatbot);
    } else {
        initChatbot();
    }
    
})();