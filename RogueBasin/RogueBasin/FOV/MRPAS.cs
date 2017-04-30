namespace RogueBasin.FOV
{
    /*
    /// <summary>
    /// This is a C# port of https://github.com/domasx2/mrpas-js/blob/master/example/javascript/mrpas.js
    /// </summary>
    class MRPAS
    {
         
    struct Tile {
        public bool wall = false;
        public bool visible = false;
    }
    
    struct PosSize {
        int x;
        int y;

        PosSize(int x, int y) {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Access using [0] == x
        /// [1] == y
        /// as in original
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        int this[int index] {

            get {
                if(index == 0)
                    return x;
                if(index == 1)
                    return y;

                throw new ApplicationException();
            }
        }

    struct Map {

        PosSize size;

        List<List<Tile>> tiles;

        Map(PosSize size) {
            
            this.size = size;
            
            tiles = new List<List<Tile>>();

            for(int x = 0 ; x < size.x; x++) {

                List<Tile> row = new List<Tile>();

                for(int y = 0 ; y < size.y; y++) {
                    row.Add(new Tile());
                }
                this.tiles.Add(row);
            }
        }

        public Tile get_tile(PosSize pos) {
            return this.tiles[pos.x][pos.y];
        }

        void set_visible(PosSize pos) {
            Tile toChange = this.tiles[pos.x][pos.y];
            toChange.visible = true;
        }

        bool is_visible(PosSize pos) {

            return this.tiles[pos.x][pos.y].visible;
        }

        bool is_transparent(PosSize pos) {

            return !this.tiles[pos.x][pos.y].wall;
        }

        void reset_visibility() {
            for(int x = 0 ; x < size.x; x++) {
                for(int y = 0 ; y < size.y; y++) {
                    Tile toChange = tiles[x][y];
                    toChange.visible = false;
                }
                }
        }


    }




    
function compute_quadrant(map, position, maxRadius, dx, dy){

    var startAngle = new Array();
    startAngle[99]=undefined;
    var endAngle = startAngle.slice(0);
    //octant: vertical edge:
    var iteration = 1;
    var done = false;
    var totalObstacles = 0;
    var obstaclesInLastLine = 0;
    var minAngle = 0.0;
    var x = 0.0;
    var y = position[1] + dy;
    var c;
    var wsize = map.size;
    
    var slopesPerCell, halfSlopes, processedCell, minx, maxx, pos, visible,
        startSlope, centerSlope, endSlope, idx;
    //do while there are unblocked slopes left and the algo is within
    // the map's boundaries
    //scan progressive lines/columns from the PC outwards
    if( (y < 0) || (y >= wsize[1])) done = true;
    while(!done){
        //process cells in the line
        slopesPerCell = 1.0 / (iteration + 1);
        halfSlopes = slopesPerCell * 0.5;
        processedCell = parseInt(minAngle / slopesPerCell);
        minx = Math.max(0, position[0] - iteration);
        maxx = Math.min(wsize[0] - 1, position[0] + iteration);
        done = true;
        x = position[0] + (processedCell * dx);
        while((x >= minx) && (x <= maxx)){
            pos = [x, y];
            visible = true;
            startSlope = processedCell * slopesPerCell;
            centreSlope = startSlope + halfSlopes;
            endSlope = startSlope + slopesPerCell;
            if((obstaclesInLastLine > 0) && (!map.is_visible(pos))){
                idx = 0;
                while(visible && (idx < obstaclesInLastLine)){
                    if(map.is_transparent(pos)){
                        if((centreSlope > startAngle[idx]) && (centreSlope < endAngle[idx]))
                            visible = false;
                    }
                    else if ((startSlope >= startAngle[idx]) && (endSlope <= endAngle[idx]))
                            visible = false;
                    if(visible && ( (!map.is_visible([x, y-dy])) ||
                              (!map.is_transparent([x, y-dy])))
                              && ((x - dx >= 0) && (x - dx < wsize[0]) &&
                              ((!map.is_visible([x-dx, y-dy]))
                               || (!map.is_transparent([x-dx, y-dy])))))
                        visible = false;
                    idx += 1;
               }
            }
            if(visible){
                map.set_visible(pos);
                done = false;
                //if the cell is opaque, block the adjacent slopes
                if(!map.is_transparent(pos)){
                    if(minAngle >= startSlope) minAngle = endSlope;
                    else{
                        startAngle[totalObstacles] = startSlope;
                        endAngle[totalObstacles] = endSlope;
                        totalObstacles += 1;
                    }
                }
            }
            processedCell += 1;
            x += dx;
        }
        if(iteration == maxRadius) done = true;
        iteration += 1
        obstaclesInLastLine = totalObstacles;
        y += dy;
        if((y < 0) || (y >= wsize[1])) done = true;
        if(minAngle == 1.0) done = true;
    }
    
    //octant: horizontal edge
    iteration = 1; //iteration of the algo for this octant
    done = false;
    totalObstacles = 0;
    obstaclesInLastLine = 0;
    minAngle = 0.0;
    x = (position[0] + dx); //the outer slope's coordinates (first processed line)
    y = 0;
    //do while there are unblocked slopes left and the algo is within the map's boundaries
    //scan progressive lines/columns from the PC outwards
    if((x < 0) || (x >= wsize[0])) done = true;
    while(!done){
        //process cells in the line
        slopesPerCell = 1.0 / (iteration + 1);
        halfSlopes = slopesPerCell * 0.5;
        processedCell = parseInt(minAngle / slopesPerCell);
        miny = Math.max(0, position[1] - iteration);
        maxy = Math.min(wsize[1] - 1, position[1] + iteration);
        done = true;
        y = position[1] + (processedCell * dy);
        while((y >= miny) && (y <= maxy)){
            //calculate slopes per cell
            pos = [x, y];
            visible = true;
            startSlope = (processedCell * slopesPerCell);
            centreSlope = startSlope + halfSlopes;
            endSlope = startSlope + slopesPerCell;
            if((obstaclesInLastLine > 0) && (!map.is_visible(pos))){
                idx = 0;
                while(visible && (idx < obstaclesInLastLine)){
                    if(map.is_transparent(pos)){
                        if((centreSlope > startAngle[idx]) && (centreSlope < endAngle[idx])) visible = false;
                    }
                    else if((startSlope >= startAngle[idx]) && (endSlope <= endAngle[idx])) visible = false;
                           
                    if(visible && (!map.is_visible([x-dx, y]) ||
                            (!map.is_transparent([x-dx, y]))) &&
                            ((y - dy >= 0) && (y - dy < wsize[1]) &&
                             ((!map.is_visible([x-dx, y-dy])) ||
                              (!map.is_transparent([x-dx, y-dy]))))) visible = false;
                    idx += 1;
               }
            }
            if(visible){
                map.set_visible(pos);
                done = false;
                //if the cell is opaque, block the adjacent slopes
                if(!map.is_transparent(pos)){
                    if(minAngle >= startSlope) minAngle = endSlope;
                    else{
                        startAngle[totalObstacles] = startSlope;
                        endAngle[totalObstacles] = endSlope;
                        totalObstacles += 1;
                    }
                }
            }
            processedCell += 1;
            y += dy;
        }
        if(iteration == maxRadius) done = true;
        iteration += 1;
        obstaclesInLastLine = totalObstacles;
        x += dx;
        if((x < 0) || (x >= wsize[0])) done = true;
        if(minAngle == 1.0) done = true;
    }
} ;


function compute(map, position, vision_range){
        map.reset_visibility();
        map.set_visible(position); //player can see himself
        //compute the 4 quadrants of the map
        this.compute_quadrant(map, position, vision_range, 1, 1);
        this.compute_quadrant(map, position, vision_range, 1, -1);
        this.compute_quadrant(map, position, vision_range, -1, 1);
        this.compute_quadrant(map, position, vision_range, -1, -1);
};

if(exports !== undefined){
    exports.compute = compute;
    exports.Map = Map;
    exports.Tile = Tile;
    exports.compute_quadrant = compute_quadrant;
}
    }*/
}
