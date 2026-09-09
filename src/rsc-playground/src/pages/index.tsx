import { Counter } from "../components/counter";

interface Product {
  id: string;
  name: string;
  price: number;
}

async function fetchProducts(): Promise<Product[]> {
  // Simulate a server-side data source.
  await new Promise((resolve) => setTimeout(resolve, 50));

  return [
    { id: "1", name: "Server-rendered runner", price: 79.99 },
    { id: "2", name: "Data-fetched sneaker", price: 129.5 },
    { id: "3", name: "Zero-JS boot", price: 45.0 },
  ];
}

export default async function HomePage() {
  const products = await fetchProducts();

  return (
    <main style={{ fontFamily: "system-ui", padding: 24 }}>
      <h1>RSC Playground</h1>

      <p>
        Rendered on the server. Data fetched inline. Zero client JS for this
        list.
      </p>

      <ul>
        {products.map((product) => (
          <li key={product.id}>
            <strong>{product.name}</strong> — ${product.price.toFixed(2)}
          </li>
        ))}
      </ul>

      <div style={{ marginTop: 24 }}>
        <Counter />
      </div>

      <p>Now compare to the SPA at http://localhost:5173.</p>
    </main>
  );
}
