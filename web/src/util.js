export const sortByProp = (prop, predicate = 'asc') => {
    return (a, b) => {
        a = a[prop];
        b = b[prop];
        
        if (predicate === 'asc') {
            if (a > b) return 1;
            if (a < b) return -1;
            return 0;
        }
        else { 
            if (a > b) return -1;
            if (a < b) return 1;
            return 0;
        }
    }
}