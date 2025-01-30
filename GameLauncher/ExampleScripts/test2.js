// @ts-check

const add = (a, b) => a + b;

const wait = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

for (let i = 0; i < 10; i++) {
    console.log(`i: ${i}`);
    await wait(1000);
}