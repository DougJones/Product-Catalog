export interface ProductSummary {
  id: number;
  title: string;
  summary: string;
  imageUrl: string;
  unitOfMeasure: string;
}

export interface ProductDetail {
  id: number;
  title: string;
  summary: string;
  description: string;
  price: string;
  imageUrl: string;
}

export interface ProductPriceMetric {
  id: number;
  title: string;
  price: string;
}

export interface CatalogMetrics {
  totalProducts: number;
  averagePrice: number;
  mostExpensive: ProductPriceMetric;
  leastExpensive: ProductPriceMetric;
  byPriceUnit: { [key: string]: number };
}
