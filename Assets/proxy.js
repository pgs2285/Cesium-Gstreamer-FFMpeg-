// proxy.js

const express = require('express');
const request = require('request');
const app = express();

let token = '';

app.use('/set-token', (req, res) => {
    token = req.query.token;  // 토큰을 URL 쿼리로 받음
    res.send('Token updated');
});

const targetUrl = 'https://demo3d.sistech3d.com';

app.use((req, res) => {
    const url = targetUrl + req.url;
    const options = {
        url: url,
        headers: {
            'Accept': 'application/json, text/plain, */*',
            'Origin': 'https://demo3d.sistech3d.com',
            'Referer': 'https://demo3d.sistech3d.com/',
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36',
            'Cookie': token  // Unity에서 전달받은 토큰 사용
        }
    };

    req.pipe(request(options)).pipe(res);
});

const port = 3000;
app.listen(port, () => {
    console.log(`Proxy server is running on http://localhost:${port}`);
});
