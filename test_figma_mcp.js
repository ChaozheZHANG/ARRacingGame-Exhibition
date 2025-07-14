#!/usr/bin/env node

/**
 * Figma MCP 配置测试脚本
 * 用于验证Figma API连接和MCP配置
 */

const { spawn } = require('child_process');
const fs = require('fs');
const path = require('path');

// 配置参数
const FIGMA_API_KEY = 'figd_6UkfAi-n5Q-K80boIY0i9gEMed0wWYgzg2H1OQxU';
const FIGMA_FILE_ID = 'hXlb6iarDjRVVpIOACIKYU'; // 从您的Figma URL提取
const MCP_CONFIG_PATH = path.join(process.env.HOME, '.cursor', 'mcp.json');

console.log('🔍 开始测试 Figma MCP 配置...\n');

// 1. 检查MCP配置文件
function checkMCPConfig() {
    console.log('📁 检查MCP配置文件...');
    
    if (!fs.existsSync(MCP_CONFIG_PATH)) {
        console.error(`❌ MCP配置文件不存在: ${MCP_CONFIG_PATH}`);
        return false;
    }
    
    try {
        const config = JSON.parse(fs.readFileSync(MCP_CONFIG_PATH, 'utf8'));
        
        if (!config.mcpServers || !config.mcpServers['Figma MCP PRO']) {
            console.error('❌ MCP配置中缺少 Figma MCP PRO 服务器配置');
            return false;
        }
        
        console.log('✅ MCP配置文件格式正确');
        return true;
    } catch (error) {
        console.error('❌ MCP配置文件解析错误:', error.message);
        return false;
    }
}

// 2. 测试Figma API连接
async function testFigmaAPI() {
    console.log('\n🌐 测试Figma API连接...');
    
    try {
        const https = require('https');
        const url = `https://api.figma.com/v1/files/${FIGMA_FILE_ID}`;
        
        return new Promise((resolve, reject) => {
            const req = https.get(url, {
                headers: {
                    'X-Figma-Token': FIGMA_API_KEY
                }
            }, (res) => {
                let data = '';
                
                res.on('data', (chunk) => {
                    data += chunk;
                });
                
                res.on('end', () => {
                    try {
                        const response = JSON.parse(data);
                        
                        if (res.statusCode === 200) {
                            console.log('✅ Figma API连接成功');
                            console.log(`📄 文件名称: ${response.name || '未知'}`);
                            console.log(`📅 最后修改: ${response.lastModified || '未知'}`);
                            resolve(true);
                        } else {
                            console.error('❌ Figma API请求失败:', response.err || response.message);
                            resolve(false);
                        }
                    } catch (parseError) {
                        console.error('❌ API响应解析错误:', parseError.message);
                        resolve(false);
                    }
                });
            });
            
            req.on('error', (error) => {
                console.error('❌ API请求错误:', error.message);
                resolve(false);
            });
            
            req.setTimeout(10000, () => {
                console.error('❌ API请求超时');
                req.destroy();
                resolve(false);
            });
        });
    } catch (error) {
        console.error('❌ Figma API测试失败:', error.message);
        return false;
    }
}

// 3. 测试figma-mcp-pro包
async function testFigmaMCPPackage() {
    console.log('\n📦 测试 figma-mcp-pro 包...');
    
    return new Promise((resolve) => {
        const child = spawn('npx', ['figma-mcp-pro@latest', '--version'], {
            stdio: ['pipe', 'pipe', 'pipe']
        });
        
        let output = '';
        let errorOutput = '';
        
        child.stdout.on('data', (data) => {
            output += data.toString();
        });
        
        child.stderr.on('data', (data) => {
            errorOutput += data.toString();
        });
        
        child.on('close', (code) => {
            if (code === 0) {
                console.log('✅ figma-mcp-pro 包可用');
                console.log(`📋 版本信息: ${output.trim()}`);
                resolve(true);
            } else {
                console.error('❌ figma-mcp-pro 包测试失败');
                if (errorOutput) {
                    console.error('错误输出:', errorOutput);
                }
                resolve(false);
            }
        });
        
        // 10秒超时
        setTimeout(() => {
            child.kill();
            console.error('❌ figma-mcp-pro 测试超时');
            resolve(false);
        }, 10000);
    });
}

// 4. 生成推荐配置
function generateRecommendedConfig() {
    console.log('\n⚙️ 生成推荐的MCP配置...');
    
    const recommendedConfig = {
        mcpServers: {
            "Figma MCP PRO": {
                command: "npx",
                args: [
                    "figma-mcp-pro@latest",
                    "--figma-api-key",
                    FIGMA_API_KEY,
                    "--timeout",
                    "30000",
                    "--cache-duration",
                    "300",
                    "--max-retries",
                    "3"
                ],
                env: {
                    DEBUG: "true",
                    NODE_ENV: "production"
                }
            }
        }
    };
    
    const configPath = 'recommended_mcp_config.json';
    fs.writeFileSync(configPath, JSON.stringify(recommendedConfig, null, 2));
    console.log(`✅ 推荐配置已保存到: ${configPath}`);
}

// 主测试流程
async function runTests() {
    let allTestsPassed = true;
    
    // 运行所有测试
    if (!checkMCPConfig()) allTestsPassed = false;
    if (!(await testFigmaAPI())) allTestsPassed = false;
    if (!(await testFigmaMCPPackage())) allTestsPassed = false;
    
    // 生成推荐配置
    generateRecommendedConfig();
    
    // 总结
    console.log('\n📊 测试总结:');
    if (allTestsPassed) {
        console.log('✅ 所有测试通过！您的Figma MCP配置应该可以正常工作。');
    } else {
        console.log('❌ 部分测试失败，请检查上述错误信息并修复配置。');
    }
    
    console.log('\n🔧 下一步操作:');
    console.log('1. 重启Cursor编辑器以加载新的MCP配置');
    console.log('2. 在对话中测试Figma文件访问功能');
    console.log('3. 如有问题，请检查API密钥权限和网络连接');
}

// 运行测试
runTests().catch(console.error); 