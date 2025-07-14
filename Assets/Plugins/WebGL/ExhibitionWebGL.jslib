mergeInto(LibraryManager.library, {

    // 请求相机权限
    RequestCameraPermission: function() {
        console.log("正在请求相机权限...");
        
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            navigator.mediaDevices.getUserMedia({ 
                video: { 
                    facingMode: 'environment',  // 后置摄像头
                    width: { ideal: 1280 },
                    height: { ideal: 720 }
                } 
            })
            .then(function(stream) {
                console.log("✅ 相机权限已获得");
                window.cameraPermissionGranted = true;
                
                // 创建video元素用于显示相机画面
                if (!window.cameraVideo) {
                    window.cameraVideo = document.createElement('video');
                    window.cameraVideo.style.position = 'fixed';
                    window.cameraVideo.style.top = '0';
                    window.cameraVideo.style.left = '0';
                    window.cameraVideo.style.width = '100%';
                    window.cameraVideo.style.height = '100%';
                    window.cameraVideo.style.objectFit = 'cover';
                    window.cameraVideo.style.zIndex = '-1';
                    window.cameraVideo.autoplay = true;
                    window.cameraVideo.playsInline = true;
                    document.body.appendChild(window.cameraVideo);
                }
                
                window.cameraVideo.srcObject = stream;
                window.cameraVideo.play();
            })
            .catch(function(error) {
                console.error("❌ 相机权限被拒绝:", error);
                window.cameraPermissionGranted = false;
                
                // 显示权限提示
                var permissionPrompt = document.createElement('div');
                permissionPrompt.innerHTML = `
                    <div style="
                        position: fixed;
                        top: 50%;
                        left: 50%;
                        transform: translate(-50%, -50%);
                        background: rgba(0,0,0,0.9);
                        color: white;
                        padding: 30px;
                        border-radius: 15px;
                        text-align: center;
                        z-index: 9999;
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
                        max-width: 90%;
                        backdrop-filter: blur(10px);
                    ">
                        <h3>🎯 需要相机权限</h3>
                        <p>请点击浏览器地址栏的📷图标<br/>或刷新页面重新授权</p>
                        <button onclick="location.reload()" style="
                            background: linear-gradient(135deg, #667eea, #764ba2);
                            color: white;
                            border: none;
                            padding: 12px 24px;
                            border-radius: 25px;
                            cursor: pointer;
                            font-size: 16px;
                            margin-top: 15px;
                        ">🔄 重新授权</button>
                    </div>
                `;
                document.body.appendChild(permissionPrompt);
            });
        } else {
            console.error("❌ 浏览器不支持相机访问");
            window.cameraPermissionGranted = false;
        }
    },

    // 检查相机权限状态
    IsCameraPermissionGranted: function() {
        return window.cameraPermissionGranted === true;
    },

    // 启用触觉反馈
    EnableVibration: function() {
        if (navigator.vibrate) {
            navigator.vibrate([50]); // 50ms振动
        } else if (window.DeviceMotionEvent && typeof DeviceMotionEvent.requestPermission === 'function') {
            // iOS 13+ 需要权限
            DeviceMotionEvent.requestPermission().then(function(permissionState) {
                if (permissionState === 'granted' && navigator.vibrate) {
                    navigator.vibrate([50]);
                }
            });
        }
    },

    // 显示全屏按钮
    ShowFullscreenButton: function() {
        if (document.fullscreenEnabled) {
            var fullscreenBtn = document.createElement('button');
            fullscreenBtn.innerHTML = '⛶';
            fullscreenBtn.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 9998;
                background: rgba(0,0,0,0.7);
                color: white;
                border: 2px solid rgba(255,255,255,0.3);
                border-radius: 50%;
                width: 50px;
                height: 50px;
                font-size: 20px;
                cursor: pointer;
                backdrop-filter: blur(10px);
                transition: all 0.3s ease;
            `;
            
            fullscreenBtn.onmouseover = function() {
                this.style.background = 'rgba(255,255,255,0.2)';
                this.style.transform = 'scale(1.1)';
            };
            
            fullscreenBtn.onmouseout = function() {
                this.style.background = 'rgba(0,0,0,0.7)';
                this.style.transform = 'scale(1)';
            };
            
            fullscreenBtn.onclick = function() {
                if (!document.fullscreenElement) {
                    document.documentElement.requestFullscreen().then(function() {
                        // 全屏成功
                        fullscreenBtn.innerHTML = '⛏';
                        
                        // 尝试启用屏幕方向锁定
                        if (screen.orientation && screen.orientation.lock) {
                            screen.orientation.lock('landscape').catch(function(error) {
                                console.log('无法锁定屏幕方向:', error);
                            });
                        }
                    });
                } else {
                    document.exitFullscreen().then(function() {
                        fullscreenBtn.innerHTML = '⛶';
                    });
                }
            };
            
            document.body.appendChild(fullscreenBtn);
            
            // 监听全屏状态变化
            document.addEventListener('fullscreenchange', function() {
                if (!document.fullscreenElement) {
                    fullscreenBtn.innerHTML = '⛶';
                }
            });
        }
    },

    // 优化展览体验
    OptimizeForExhibition: function() {
        console.log("🎪 优化展览体验...");
        
        // 禁用上下文菜单
        document.addEventListener('contextmenu', function(e) {
            e.preventDefault();
        });
        
        // 禁用文本选择
        document.addEventListener('selectstart', function(e) {
            e.preventDefault();
        });
        
        // 禁用图片拖拽
        document.addEventListener('dragstart', function(e) {
            e.preventDefault();
        });
        
        // 自动隐藏地址栏（移动端）
        window.addEventListener('load', function() {
            setTimeout(function() {
                window.scrollTo(0, 1);
            }, 1000);
        });
        
        // 防止页面缩放
        document.addEventListener('touchmove', function(e) {
            if (e.touches.length > 1) {
                e.preventDefault();
            }
        }, { passive: false });
        
        var lastTouchEnd = 0;
        document.addEventListener('touchend', function(e) {
            var now = (new Date()).getTime();
            if (now - lastTouchEnd <= 300) {
                e.preventDefault();
            }
            lastTouchEnd = now;
        }, false);
        
        // 添加展览说明overlay
        var exhibitionOverlay = document.createElement('div');
        exhibitionOverlay.innerHTML = `
            <div id="exhibition-welcome" style="
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: linear-gradient(135deg, rgba(102, 126, 234, 0.95), rgba(118, 75, 162, 0.95));
                z-index: 10000;
                display: flex;
                flex-direction: column;
                justify-content: center;
                align-items: center;
                color: white;
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
                text-align: center;
                backdrop-filter: blur(10px);
            ">
                <div style="max-width: 80%; padding: 40px;">
                    <h1 style="font-size: 3em; margin-bottom: 20px; text-shadow: 0 2px 4px rgba(0,0,0,0.3);">
                        🏎️ ScriptEcho AR赛车
                    </h1>
                    <h2 style="font-size: 1.5em; margin-bottom: 30px; opacity: 0.9;">
                        沉浸式AR赛车体验
                    </h2>
                    <div style="font-size: 1.2em; line-height: 1.6; margin-bottom: 40px;">
                        <p>🎯 点击屏幕放置虚拟赛车</p>
                        <p>🏁 触摸控制驾驶体验</p>
                        <p>🎮 收集所有旗帜获胜</p>
                    </div>
                    <button onclick="document.getElementById('exhibition-welcome').style.display='none'" style="
                        background: white;
                        color: #667eea;
                        border: none;
                        padding: 15px 40px;
                        border-radius: 30px;
                        font-size: 1.2em;
                        font-weight: bold;
                        cursor: pointer;
                        box-shadow: 0 4px 15px rgba(0,0,0,0.2);
                        transition: all 0.3s ease;
                    " onmouseover="this.style.transform='scale(1.05)'" onmouseout="this.style.transform='scale(1)'">
                        🚀 开始体验
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(exhibitionOverlay);
        
        console.log("✅ 展览优化完成");
    },

    // 检测设备性能并调整质量
    DetectAndOptimizePerformance: function() {
        var canvas = document.querySelector('#unity-canvas');
        if (!canvas) return;
        
        // 检测设备性能
        var devicePixelRatio = window.devicePixelRatio || 1;
        var screenWidth = window.screen.width;
        var isLowEndDevice = screenWidth < 768 || navigator.hardwareConcurrency < 4;
        
        if (isLowEndDevice) {
            console.log("🔧 检测到低端设备，应用性能优化...");
            
            // 降低渲染分辨率
            canvas.style.imageRendering = 'pixelated';
            
            // 添加性能提示
            var performanceHint = document.createElement('div');
            performanceHint.innerHTML = '⚡ 已优化性能设置';
            performanceHint.style.cssText = `
                position: fixed;
                bottom: 10px;
                left: 10px;
                background: rgba(0,0,0,0.7);
                color: white;
                padding: 8px 15px;
                border-radius: 15px;
                font-size: 12px;
                z-index: 9999;
            `;
            document.body.appendChild(performanceHint);
            
            setTimeout(function() {
                performanceHint.remove();
            }, 3000);
        }
    },

    // 展示二维码（供分享）
    ShowQRCode: function(urlPtr) {
        var url = UTF8ToString(urlPtr);
        
        // 创建二维码显示
        var qrOverlay = document.createElement('div');
        qrOverlay.innerHTML = `
            <div style="
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0,0,0,0.9);
                z-index: 10001;
                display: flex;
                justify-content: center;
                align-items: center;
                color: white;
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
            ">
                <div style="text-align: center; padding: 40px;">
                    <h2>📱 扫码体验</h2>
                    <div id="qr-code" style="
                        background: white;
                        padding: 20px;
                        border-radius: 15px;
                        margin: 20px auto;
                        display: inline-block;
                    "></div>
                    <p>使用手机扫描二维码<br/>即可体验AR赛车游戏</p>
                    <button onclick="this.parentElement.parentElement.remove()" style="
                        background: #667eea;
                        color: white;
                        border: none;
                        padding: 12px 24px;
                        border-radius: 25px;
                        cursor: pointer;
                        font-size: 16px;
                        margin-top: 20px;
                    ">关闭</button>
                </div>
            </div>
        `;
        document.body.appendChild(qrOverlay);
        
        // 这里可以集成第三方二维码生成库
        // 例如 qrcode.js
        document.getElementById('qr-code').innerHTML = `
            <div style="
                width: 200px;
                height: 200px;
                background: #f0f0f0;
                display: flex;
                align-items: center;
                justify-content: center;
                color: #666;
                font-size: 14px;
            ">
                ${url}
            </div>
        `;
    }

}); 