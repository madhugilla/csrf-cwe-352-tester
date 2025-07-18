# CSRF Testing Sites for Your Tool

This document provides a comprehensive list of sample websites where you can test your CSRF vulnerability testing tool safely and legally.

## ⚠️ IMPORTANT WARNINGS

**NEVER test on live websites without explicit permission!** Only use the sites listed below which are specifically designed for security testing.

## � PUBLICLY AVAILABLE TESTING SITES (No Docker Required!)

### 1. Acunetix Test Site (⭐ HIGHLY RECOMMENDED)
**Best for:** Live CSRF testing without any setup

- **URL:** http://testphp.vulnweb.com/
- **Access:** Direct browser access - no installation needed
- **Features:** 
  - Login forms at http://testphp.vulnweb.com/login.php
  - User profile forms at http://testphp.vulnweb.com/userinfo.php
  - Guestbook forms at http://testphp.vulnweb.com/guestbook.php
  - Shopping cart at http://testphp.vulnweb.com/cart.php
- **CSRF Testing:** Multiple forms with various protection levels
- **Note:** Specifically mentions CSRF in their testing documentation

### 2. Altoro Mutual Demo Bank (⭐ EXCELLENT FOR BANKING FORMS)
**Best for:** Real-world banking form CSRF testing

- **URL:** https://demo.testfire.net/
- **Access:** Direct browser access - no installation needed
- **Features:**
  - Login forms at https://demo.testfire.net/login.jsp
  - Account management forms
  - Transfer forms
  - Contact forms at https://demo.testfire.net/feedback.jsp
- **CSRF Testing:** Banking-style forms with various token implementations
- **Note:** Professional demo site maintained by HCL for security testing

### 3. PortSwigger Web Security Academy
**Best for:** Structured CSRF learning labs

- **URL:** https://portswigger.net/web-security/csrf
- **Access:** Free account required
- **Features:**
  - Interactive CSRF labs
  - Step-by-step vulnerability scenarios
  - Multiple difficulty levels
- **CSRF Testing:** Professional-grade lab environment

### 4. Security Shepherd (OWASP)
**Best for:** Gamified security challenges

- **URL:** https://security-shepherd.ctf.town/ (when available)
- **Access:** Free registration
- **Features:** CSRF challenges in a game-like environment
- **Note:** May require registration, check availability

## 🧪 Test Scenarios to Try

### Basic CSRF Tests
1. **Simple GET Request CSRF**
   - URL: `http://localhost:8080/WebGoat/csrf/basic-get-flag`
   - Test: Submit without CSRF token

2. **POST Form CSRF**
   - URL: `http://localhost:4280/vulnerabilities/csrf/`
   - Test: Password change form manipulation

3. **Hidden Token Bypass**
   - Test: Forms with hidden CSRF tokens
   - Verify detection of missing/invalid tokens

### Advanced CSRF Tests
1. **JSON CSRF**
   - Content-Type: application/json
   - Test: API endpoints with JSON payloads

2. **Multi-Step CSRF**
   - Test: Forms requiring multiple steps
   - Verify token persistence across steps

3. **SameSite Cookie Bypass**
   - Test: Different SameSite cookie settings
   - Verify bypass techniques

## 🚀 Quick Start Testing Commands

Test your tool against these live websites:

```powershell
# Test Acunetix Test Site (No authentication needed)
dotnet run -- --url http://testphp.vulnweb.com/login.php --headless false --verbose

# Test Acunetix guestbook form
dotnet run -- --url http://testphp.vulnweb.com/guestbook.php --headless false --verbose

# Test Altoro Mutual banking forms
dotnet run -- --url https://demo.testfire.net/login.jsp --headless false --verbose

# Test Altoro feedback form
dotnet run -- --url https://demo.testfire.net/feedback.jsp --headless false --verbose

# Output results to file
dotnet run -- --url http://testphp.vulnweb.com/login.php --output csrf_results.json --verbose

# Test with custom user agent (some sites may require this)
dotnet run -- --url https://demo.testfire.net/login.jsp --headless false --verbose
```

## 📝 Expected Test Results

Your tool should detect:

### ✅ Positive Cases (Vulnerabilities Found)
- Forms without CSRF tokens
- Forms with predictable tokens
- Forms accepting GET requests for state changes
- Missing token validation
- Token reuse across sessions

### ❌ Negative Cases (No Vulnerabilities)
- Forms with proper CSRF tokens
- Correct token validation
- Proper SameSite cookie settings
- Double-submit cookie patterns

## 🛠️ Getting Started (No Setup Required!)

### Step 1: Verify Your Tool Builds
```powershell
# Ensure your CSRF tool compiles
dotnet build

# Install Microsoft Edge for Playwright (if not already done)
playwright install msedge
```

### Step 2: Test Against Live Sites

Start with the simplest site first:

```powershell
# Test Acunetix (simplest, no authentication)
dotnet run -- --url http://testphp.vulnweb.com/login.php --headless false --verbose
```

### Step 3: Analyze Results

Your tool should detect forms and analyze CSRF protection on these live sites.

## 🔍 Testing Checklist

Before running your CSRF tool, ensure:

- [ ] Target application is running locally
- [ ] You can access the application in browser
- [ ] Microsoft Edge is installed (`playwright install msedge`)
- [ ] Your tool builds without errors (`dotnet build`)
- [ ] Test with `--headless false` first to see what's happening
- [ ] Use `--verbose` flag for detailed logging

## 📊 Sample Test Reports

Your tool should generate reports showing:

```json
{
  "url": "http://localhost:8080/WebGoat/csrf",
  "timestamp": "2025-07-18T10:30:00Z",
  "vulnerabilities": [
    {
      "type": "Missing CSRF Token",
      "severity": "High",
      "form": "#password-change-form",
      "method": "POST",
      "action": "/WebGoat/csrf/basic-get-flag",
      "description": "Form submits without CSRF protection"
    }
  ],
  "formsAnalyzed": 3,
  "vulnerabilitiesFound": 1
}
```

## 🔗 Additional Resources

- **OWASP CSRF Prevention:** https://owasp.org/www-community/attacks/csrf
- **WebGoat Documentation:** https://owasp.org/www-project-webgoat/
- **DVWA Guide:** https://github.com/digininja/DVWA
- **Playwright Documentation:** https://playwright.dev/dotnet/

## ⚡ Quick Troubleshooting

### Common Issues:

1. **"Browser not found"** → Run `playwright install msedge`
2. **"Connection refused"** → Check if the website is accessible in your regular browser first
3. **"Timeout"** → Increase timeout or check if the site is responding slowly
4. **"Build errors"** → Ensure all NuGet packages are installed with `dotnet restore`

### Testing Tips:

- Always test with `--headless false` first to see what your tool is doing
- Use `--verbose` flag for detailed logging
- Start with simple sites like Acunetix before moving to more complex ones
- Some sites may block automated tools - this is normal and part of the testing

Happy testing! 🎯🔒
