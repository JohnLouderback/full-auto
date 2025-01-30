const add = (a: number, b: number): number => a + b;

const wait = (ms: number): Promise<void> => new Promise((resolve) => setTimeout(resolve, ms));

for (let i = 0; i < 10; i++) {
    console.log(`i: ${i}`);
    await wait(1);
}

export {};