// Volumetric light rays on a WebGL canvas — a port of React Bits' LightRays
// (reactbits.dev/backgrounds/light-rays): the same shader and inputs, on plain WebGL instead of
// ogl, with the light carried in alpha so the colour stays pure over any ground. Frames come
// from GSAP's ticker, and only while the rays are on screen. Under reduced motion it paints one
// still frame; without WebGL it renders nothing and the ground shows through.
//
//   <div class="light-rays" data-light-rays data-rays-color="var(--rail-accent)"></div>
//   data-rays-color       any CSS colour, including var(--token)
//   data-rays-origin      top-center | top-left | top-right | left | right |
//                         bottom-center | bottom-left | bottom-right
//   data-rays-speed       1     data-rays-spread  1     data-rays-length  2
//   data-rays-fade        1     data-rays-noise   0     data-rays-distortion  0
//   data-rays-saturation  1     data-rays-mouse   0.1 (how far the beams lean toward the cursor; 0 = off)
(function () {
    'use strict';

    var gsap = window.gsap;

    var VERTEX = [
        'attribute vec2 position;',
        'void main() {',
        '  gl_Position = vec4(position, 0.0, 1.0);',
        '}'
    ].join('\n');

    var FRAGMENT = [
        'precision highp float;',

        'uniform float iTime;',
        'uniform vec2  iResolution;',
        'uniform vec2  rayPos;',
        'uniform vec2  rayDir;',
        'uniform vec3  raysColor;',
        'uniform float raysSpeed;',
        'uniform float lightSpread;',
        'uniform float rayLength;',
        'uniform float pulsating;',
        'uniform float fadeDistance;',
        'uniform float saturation;',
        'uniform vec2  mousePos;',
        'uniform float mouseInfluence;',
        'uniform float noiseAmount;',
        'uniform float distortion;',

        'float noise(vec2 st) {',
        '  return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);',
        '}',

        'float rayStrength(vec2 raySource, vec2 rayRefDirection, vec2 coord, float seedA, float seedB, float speed) {',
        '  vec2 sourceToCoord = coord - raySource;',
        '  vec2 dirNorm = normalize(sourceToCoord);',
        '  float cosAngle = dot(dirNorm, rayRefDirection);',
        '  float distortedAngle = cosAngle + distortion * sin(iTime * 2.0 + length(sourceToCoord) * 0.01) * 0.2;',
        '  float spreadFactor = pow(max(distortedAngle, 0.0), 1.0 / max(lightSpread, 0.001));',
        '  float distance = length(sourceToCoord);',
        '  float maxDistance = iResolution.x * rayLength;',
        '  float lengthFalloff = clamp((maxDistance - distance) / maxDistance, 0.0, 1.0);',
        '  float fadeFalloff = clamp((iResolution.x * fadeDistance - distance) / (iResolution.x * fadeDistance), 0.5, 1.0);',
        '  float pulse = pulsating > 0.5 ? (0.8 + 0.2 * sin(iTime * speed * 3.0)) : 1.0;',
        '  float baseStrength = clamp(',
        '    (0.45 + 0.15 * sin(distortedAngle * seedA + iTime * speed)) +',
        '    (0.3 + 0.2 * cos(-distortedAngle * seedB + iTime * speed)),',
        '    0.0, 1.0',
        '  );',
        '  return baseStrength * lengthFalloff * fadeFalloff * spreadFactor * pulse;',
        '}',

        'void main() {',
        '  vec2 coord = vec2(gl_FragCoord.x, iResolution.y - gl_FragCoord.y);',

        '  vec2 finalRayDir = rayDir;',
        '  if (mouseInfluence > 0.0) {',
        '    vec2 mouseScreenPos = mousePos * iResolution.xy;',
        '    vec2 mouseDirection = normalize(mouseScreenPos - rayPos);',
        '    finalRayDir = normalize(mix(rayDir, mouseDirection, mouseInfluence));',
        '  }',

        '  vec4 rays1 = vec4(1.0) * rayStrength(rayPos, finalRayDir, coord, 36.2214, 21.11349, 1.5 * raysSpeed);',
        '  vec4 rays2 = vec4(1.0) * rayStrength(rayPos, finalRayDir, coord, 22.3991, 18.0234, 1.1 * raysSpeed);',
        '  vec4 color = rays1 * 0.5 + rays2 * 0.4;',

        '  if (noiseAmount > 0.0) {',
        '    float n = noise(coord * 0.01 + iTime * 0.1);',
        '    color.rgb *= (1.0 - noiseAmount + noiseAmount * n);',
        '  }',

        '  float brightness = 1.0 - (coord.y / iResolution.y);',
        '  color.x *= 0.1 + brightness * 0.8;',
        '  color.y *= 0.3 + brightness * 0.6;',
        '  color.z *= 0.5 + brightness * 0.5;',

        '  if (saturation != 1.0) {',
        '    float gray = dot(color.rgb, vec3(0.299, 0.587, 0.114));',
        '    color.rgb = mix(vec3(gray), color.rgb, saturation);',
        '  }',

        // Port change: the original multiplies the colour down, which only reads as light over
        // a dark ground. Carrying the light in alpha keeps the colour pure over any ground.
        '  gl_FragColor = vec4(raysColor, max(color.r, max(color.g, color.b)));',
        '}'
    ].join('\n');

    var UNIFORMS = [
        'iTime', 'iResolution', 'rayPos', 'rayDir', 'raysColor', 'raysSpeed', 'lightSpread',
        'rayLength', 'pulsating', 'fadeDistance', 'saturation', 'mousePos', 'mouseInfluence',
        'noiseAmount', 'distortion'
    ];

    // Any frame works as the still; this one has the beams well spread.
    var STILL_TIME = 2.4;

    function anchorAndDir(origin, w, h) {
        var outside = 0.2;
        switch (origin) {
            case 'top-left': return { anchor: [0, -outside * h], dir: [0, 1] };
            case 'top-right': return { anchor: [w, -outside * h], dir: [0, 1] };
            case 'left': return { anchor: [-outside * w, 0.5 * h], dir: [1, 0] };
            case 'right': return { anchor: [(1 + outside) * w, 0.5 * h], dir: [-1, 0] };
            case 'bottom-left': return { anchor: [0, (1 + outside) * h], dir: [0, -1] };
            case 'bottom-center': return { anchor: [0.5 * w, (1 + outside) * h], dir: [0, -1] };
            case 'bottom-right': return { anchor: [w, (1 + outside) * h], dir: [0, -1] };
            default: return { anchor: [0.5 * w, -outside * h], dir: [0, 1] };
        }
    }

    function option(el, name, fallback) {
        var value = parseFloat(el.getAttribute('data-rays-' + name));
        return isFinite(value) ? value : fallback;
    }

    function compile(gl, type, source) {
        var shader = gl.createShader(type);
        gl.shaderSource(shader, source);
        gl.compileShader(shader);
        return gl.getShaderParameter(shader, gl.COMPILE_STATUS) ? shader : null;
    }

    function createProgram(gl) {
        var vertex = compile(gl, gl.VERTEX_SHADER, VERTEX);
        var fragment = compile(gl, gl.FRAGMENT_SHADER, FRAGMENT);
        if (!vertex || !fragment) return null;

        var program = gl.createProgram();
        gl.attachShader(program, vertex);
        gl.attachShader(program, fragment);
        gl.linkProgram(program);
        return gl.getProgramParameter(program, gl.LINK_STATUS) ? program : null;
    }

    window.plMotion.component('[data-light-rays]', function (host) {
        var canvas = document.createElement('canvas');
        var gl = null;
        try {
            gl = canvas.getContext('webgl', { alpha: true, premultipliedAlpha: false, antialias: false });
        } catch (e) { /* no WebGL: the ground shows through */ }
        if (!gl) return;

        var program = createProgram(gl);
        if (!program) return;
        gl.useProgram(program);

        // One triangle that covers the whole canvas.
        gl.bindBuffer(gl.ARRAY_BUFFER, gl.createBuffer());
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([-1, -1, 3, -1, -1, 3]), gl.STATIC_DRAW);
        var position = gl.getAttribLocation(program, 'position');
        gl.enableVertexAttribArray(position);
        gl.vertexAttribPointer(position, 2, gl.FLOAT, false, 0, 0);

        var u = {};
        UNIFORMS.forEach(function (name) { u[name] = gl.getUniformLocation(program, name); });

        gl.uniform1f(u.raysSpeed, option(host, 'speed', 1));
        gl.uniform1f(u.lightSpread, option(host, 'spread', 1));
        gl.uniform1f(u.rayLength, option(host, 'length', 2));
        gl.uniform1f(u.pulsating, 0);
        gl.uniform1f(u.fadeDistance, option(host, 'fade', 1));
        gl.uniform1f(u.saturation, option(host, 'saturation', 1));
        gl.uniform1f(u.noiseAmount, option(host, 'noise', 0));
        gl.uniform1f(u.distortion, option(host, 'distortion', 0));

        // The colour is resolved by the browser, so design tokens work as the value.
        host.style.color = host.getAttribute('data-rays-color') || '#ffffff';
        var rgb = (getComputedStyle(host).color.match(/[\d.]+/g) || [255, 255, 255]).map(Number);
        gl.uniform3f(u.raysColor, rgb[0] / 255, rgb[1] / 255, rgb[2] / 255);

        var origin = host.getAttribute('data-rays-origin') || 'top-center';
        var mouseInfluence = option(host, 'mouse', 0.1);

        canvas.style.cssText = 'display:block;width:100%;height:100%';
        host.appendChild(canvas);

        var time = STILL_TIME;
        var influence = 0;
        var mouse = [0.5, 0.5];
        var smooth = [0.5, 0.5];
        var animating = false;
        var visible = false;
        var running = false;

        function draw() {
            gl.uniform1f(u.iTime, time);
            gl.uniform2f(u.mousePos, smooth[0], smooth[1]);
            gl.uniform1f(u.mouseInfluence, influence);
            gl.drawArrays(gl.TRIANGLES, 0, 3);
        }

        function tick(seconds) {
            time = seconds;
            smooth[0] = smooth[0] * 0.92 + mouse[0] * 0.08;
            smooth[1] = smooth[1] * 0.92 + mouse[1] * 0.08;
            draw();
        }

        function still() {
            time = STILL_TIME;
            influence = 0;
            smooth[0] = smooth[1] = 0.5;
            draw();
        }

        // Frames only while motion is on and the rays are on screen.
        function sync() {
            var on = animating && visible;
            if (on === running) return;
            running = on;
            if (on) gsap.ticker.add(tick);
            else gsap.ticker.remove(tick);
        }

        function resize() {
            var w = host.clientWidth;
            var h = host.clientHeight;
            if (!w || !h) return;
            var dpr = Math.min(window.devicePixelRatio || 1, 2);
            canvas.width = Math.round(w * dpr);
            canvas.height = Math.round(h * dpr);
            gl.viewport(0, 0, canvas.width, canvas.height);

            var ray = anchorAndDir(origin, canvas.width, canvas.height);
            gl.uniform2f(u.iResolution, canvas.width, canvas.height);
            gl.uniform2f(u.rayPos, ray.anchor[0], ray.anchor[1]);
            gl.uniform2f(u.rayDir, ray.dir[0], ray.dir[1]);
            // Resizing clears the canvas; repaint it if the ticker isn't about to.
            if (!running) draw();
        }

        function onMouseMove(e) {
            var r = host.getBoundingClientRect();
            mouse[0] = (e.clientX - r.left) / r.width;
            mouse[1] = (e.clientY - r.top) / r.height;
        }

        new ResizeObserver(resize).observe(host);
        resize();
        new IntersectionObserver(function (entries) {
            visible = entries[0].isIntersecting;
            sync();
        }).observe(host);

        window.plMotion.whenMotion(
            function () {
                animating = true;
                influence = mouseInfluence;
                if (influence > 0) window.addEventListener('mousemove', onMouseMove, { passive: true });
                sync();
                gsap.from(canvas, { opacity: 0, duration: 1.6, ease: 'power2.out' });
                return function () {
                    animating = false;
                    window.removeEventListener('mousemove', onMouseMove);
                    sync();
                    still();
                };
            },
            still
        );
    });
})();
