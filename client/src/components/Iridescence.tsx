import { Renderer, Program, Mesh, Color, Triangle } from 'ogl';
import { useEffect, useRef, useCallback } from 'react';

const vertexShader = `
attribute vec2 uv;
attribute vec2 position;

varying vec2 vUv;

void main() {
  vUv = uv;
  gl_Position = vec4(position, 0, 1);
}
`;

const fragmentShader = `
precision highp float;

uniform float uTime;
uniform vec3 uColor;
uniform vec3 uResolution;
uniform vec2 uMouse;
uniform float uAmplitude;
uniform float uSpeed;

varying vec2 vUv;

void main() {
  float mr = min(uResolution.x, uResolution.y);
  vec2 uv = (vUv.xy * 2.0 - 1.0) * uResolution.xy / mr;

  uv += (uMouse - vec2(0.5)) * uAmplitude;

  float d = -uTime * 0.5 * uSpeed;
  float a = 0.0;
  for (float i = 0.0; i < 8.0; ++i) {
    a += cos(i - d - a * uv.x);
    d += sin(uv.y * i + a);
  }
  d += uTime * 0.5 * uSpeed;
  vec3 col = vec3(cos(uv * vec2(d, a)) * 0.6 + 0.4, cos(a + d) * 0.5 + 0.5);
  col = cos(col * cos(vec3(d, a, 2.5)) * 0.5 + 0.5) * uColor;
  gl_FragColor = vec4(col, 1.0);
}
`;

function isDarkMode() {
  return document.documentElement.getAttribute('data-theme') === 'dark';
}

function getThemeColor(): [number, number, number] {
  return isDarkMode() ? [0.14, 0.14, 0.16] : [0.88, 0.87, 0.86];
}

function getClearColor(): [number, number, number, number] {
  return isDarkMode() ? [0.055, 0.055, 0.067, 1] : [1, 1, 1, 1];
}

interface IridescenceProps {
  speed?: number;
  amplitude?: number;
  mouseReact?: boolean;
}

export default function Iridescence({
  speed = 0.8,
  amplitude = 0.1,
  mouseReact = true,
  ...rest
}: IridescenceProps) {
  const ctnDom = useRef<HTMLDivElement>(null);
  const mousePos = useRef({ x: 0.5, y: 0.5 });

  const initGL = useCallback(() => {
    if (!ctnDom.current) return;
    const ctn = ctnDom.current;
    const renderer = new Renderer();
    const gl = renderer.gl;
    const clear = getClearColor();
    gl.clearColor(clear[0], clear[1], clear[2], clear[3]);

    let program: Program;

    function resize() {
      renderer.setSize(ctn.offsetWidth, ctn.offsetHeight);
      if (program) {
        program.uniforms.uResolution.value = new Color(
          gl.canvas.width,
          gl.canvas.height,
          gl.canvas.width / gl.canvas.height
        );
      }
    }
    window.addEventListener('resize', resize, false);
    resize();

    const geometry = new Triangle(gl);
    const themeColor = getThemeColor();
    program = new Program(gl, {
      vertex: vertexShader,
      fragment: fragmentShader,
      uniforms: {
        uTime: { value: 0 },
        uColor: { value: new Color(...themeColor) },
        uResolution: {
          value: new Color(gl.canvas.width, gl.canvas.height, gl.canvas.width / gl.canvas.height)
        },
        uMouse: { value: new Float32Array([mousePos.current.x, mousePos.current.y]) },
        uAmplitude: { value: amplitude },
        uSpeed: { value: speed }
      }
    });

    const mesh = new Mesh(gl, { geometry, program });
    let animateId: number;

    function update(t: number) {
      animateId = requestAnimationFrame(update);
      program.uniforms.uTime.value = t * 0.001;
      renderer.render({ scene: mesh });
    }
    animateId = requestAnimationFrame(update);
    ctn.appendChild(gl.canvas);

    function handleMouseMove(e: MouseEvent) {
      const rect = ctn.getBoundingClientRect();
      const x = (e.clientX - rect.left) / rect.width;
      const y = 1.0 - (e.clientY - rect.top) / rect.height;
      mousePos.current = { x, y };
      program.uniforms.uMouse.value[0] = x;
      program.uniforms.uMouse.value[1] = y;
    }

    const mouseTarget = ctn.parentElement || ctn;
    if (mouseReact) {
      mouseTarget.addEventListener('mousemove', handleMouseMove);
    }

    const observer = new MutationObserver(() => {
      const dark = isDarkMode();
      const c = dark ? [0.14, 0.14, 0.16] : [0.88, 0.87, 0.86];
      const bg = dark ? [0.055, 0.055, 0.067, 1] : [1, 1, 1, 1];
      program.uniforms.uColor.value = new Color(c[0], c[1], c[2]);
      gl.clearColor(bg[0], bg[1], bg[2], bg[3]);
    });
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] });

    return () => {
      cancelAnimationFrame(animateId);
      window.removeEventListener('resize', resize);
      observer.disconnect();
      if (mouseReact) {
        mouseTarget.removeEventListener('mousemove', handleMouseMove);
      }
      ctn.removeChild(gl.canvas);
      gl.getExtension('WEBGL_lose_context')?.loseContext();
    };
  }, [speed, amplitude, mouseReact]);

  useEffect(() => {
    const cleanup = initGL();
    return cleanup;
  }, [initGL]);

  return <div ref={ctnDom} className="iridescence-container" {...rest} />;
}
